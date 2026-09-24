using Admin.NET.Application101.Dtos.Operations;
using Admin.NET.Core101.Domain;
using Npgsql;

namespace Admin.NET.Application101.Services;

public sealed class OperationService101(
    SqlSugarRepository<Operation101> operations,
    ICurrentUser101 currentUser) : ITransient
{
    public async Task<IReadOnlyList<OperationDto>> ListAsync(Guid taskId)
    {
        await FindTaskAsync(taskId);
        var rows = await operations.AsQueryable().Where(item => item.TaskId == taskId)
            .OrderBy(item => item.OrderNo).ToListAsync();
        return rows.Select(Map).ToArray();
    }

    public async Task<OperationDto> GetAsync(Guid id)
    {
        var operation = await FindOperationAsync(id);
        var checks = await operations.Context.Queryable<OperationCheck101>().Where(item => item.OperationId == id)
            .OrderBy(item => item.OrderNo).ToListAsync();
        var signatures = await operations.Context.Queryable<OperationSignature101>()
            .Where(item => item.Scope == "operation" && item.ScopeId == id).OrderBy(item => item.SignedAt).ToListAsync();
        var dto = Map(operation);
        dto.Checks = checks.Select(item => new OperationCheckDto(item.Id, item.Item, item.Requirement,
            item.Actual, item.Remark, item.OrderNo)).ToArray();
        dto.Signatures = signatures.Select(item => new OperationSignatureDto(item.Id, item.Role,
            item.SignerUserId, item.SignerName, item.SignedAt)).ToArray();
        return dto;
    }

    public async Task UpdateAsync(Guid id, UpdateOperationInput input)
    {
        var operation = await FindMutableOperationAsync(id);
        operation.Code = input.Code.Trim(); operation.OperationDate = input.Date;
        operation.QualityRequirement = input.QualityRequirement.Trim();
        operation.OperationRequirement = input.OperationRequirement.Trim();
        operation.Attention = input.Attention.Trim(); operation.Status = input.Status;
        operation.UpdateTime = DateTime.UtcNow;
        await operations.AsUpdateable(operation).ExecuteCommandAsync();
    }

    public async Task SaveChecksAsync(Guid id, SaveOperationChecksInput input)
    {
        await FindMutableOperationAsync(id);
        var ids = input.Rows.Where(item => item.Id.HasValue).Select(item => item.Id!.Value).ToArray();
        if (ids.Distinct().Count() != ids.Length) throw Oops.Oh("检查项 ID 不能重复。").StatusCode(400);
        var database = operations.Context;
        await database.Ado.BeginTranAsync();
        try
        {
            await database.Deleteable<OperationCheck101>().Where(item => item.OperationId == id).ExecuteCommandAsync();
            var rows = input.Rows.OrderBy(item => item.Order).Select(item => new OperationCheck101
            {
                Id = item.Id ?? Guid.NewGuid(), OperationId = id, Item = item.Item.Trim(),
                Requirement = item.Requirement.Trim(), Actual = item.Actual.Trim(), Remark = item.Remark.Trim(),
                OrderNo = item.Order
            }).ToList();
            if (rows.Count > 0) await database.Insertable(rows).ExecuteCommandAsync();
            await database.Ado.CommitTranAsync();
        }
        catch
        {
            await database.Ado.RollbackTranAsync();
            throw;
        }
    }

    public async Task<OperationSignatureDto> SignAsync(Guid operationId, CreateSignatureInput input)
    {
        if (!SignatureRoles.Operation.Contains(input.Role))
            throw Oops.Oh("不支持的签名角色。").StatusCode(400);
        await FindMutableOperationAsync(operationId);
        var database = operations.Context;
        if (await database.Queryable<OperationSignature101>().AnyAsync(item =>
                item.Scope == "operation" && item.ScopeId == operationId && item.Role == input.Role))
            throw Oops.Oh("该角色已签名。").StatusCode(409);
        if (await database.Queryable<OperationSignature101>().AnyAsync(item =>
                item.Scope == "operation" && item.ScopeId == operationId && item.SignerUserId == currentUser.UserId))
            throw Oops.Oh("当前账号已在此位置签名。").StatusCode(409);
        var signature = new OperationSignature101
        {
            Scope = "operation", ScopeId = operationId, Role = input.Role,
            SignerUserId = currentUser.UserId, SignerName = currentUser.RealName, SignedAt = DateTime.UtcNow
        };
        try
        {
            await database.Insertable(signature).ExecuteCommandAsync();
        }
        catch (PostgresException error) when (error.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw Oops.Oh("该角色或账号已在此位置签名。").StatusCode(409);
        }
        return new OperationSignatureDto(signature.Id, signature.Role, signature.SignerUserId,
            signature.SignerName, signature.SignedAt);
    }

    public async Task WithdrawSignatureAsync(Guid operationId, string role)
    {
        await FindMutableOperationAsync(operationId);
        var signature = await operations.Context.Queryable<OperationSignature101>().FirstAsync(item =>
            item.Scope == "operation" && item.ScopeId == operationId && item.Role == role)
            ?? throw Oops.Oh("记录不存在。").StatusCode(404);
        if (signature.SignerUserId != currentUser.UserId && !currentUser.IsAdministrator)
            throw Oops.Oh("只能撤回自己的签名。").StatusCode(403);
        await operations.Context.Deleteable<OperationSignature101>().In(signature.Id).ExecuteCommandAsync();
    }

    private async Task<Operation101> FindOperationAsync(Guid id) =>
        await operations.GetFirstAsync(item => item.Id == id) ?? throw Oops.Oh("记录不存在。").StatusCode(404);

    private async Task<Operation101> FindMutableOperationAsync(Guid id)
    {
        var operation = await FindOperationAsync(id);
        var task = await FindTaskAsync(operation.TaskId);
        EnsureMutable(task.Status);
        return operation;
    }

    private async Task<Task101> FindTaskAsync(Guid id) =>
        await operations.Context.Queryable<Task101>().FirstAsync(item => item.Id == id)
        ?? throw Oops.Oh("记录不存在。").StatusCode(404);

    private static void EnsureMutable(TaskStatus101 status)
    {
        try { TaskWriteGuard.EnsureMutable(status); }
        catch (InvalidOperationException error) { throw Oops.Oh(error.Message).StatusCode(409); }
    }

    private static OperationDto Map(Operation101 item) => new()
    {
        Id = item.Id, TaskId = item.TaskId, WorkflowNodeId = item.WorkflowNodeId, Code = item.Code,
        Phase = item.Phase, Process = item.Process, Step = item.Step, Post = item.Post,
        Date = item.OperationDate, QualityRequirement = item.QualityRequirement,
        OperationRequirement = item.OperationRequirement, Attention = item.Attention,
        Status = item.Status, UpdatedAt = item.UpdateTime ?? item.CreateTime
    };
}

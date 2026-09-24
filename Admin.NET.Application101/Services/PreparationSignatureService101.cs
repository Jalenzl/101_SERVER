using Admin.NET.Application101.Dtos.Records;
using Admin.NET.Application101.Validation;
using Admin.NET.Core101.Domain;
using Npgsql;

namespace Admin.NET.Application101.Services;

public sealed class PreparationSignatureService101(
    SqlSugarRepository<Task101> tasks, ICurrentUser101 currentUser) : ITransient
{
    public async Task<IReadOnlyList<SignatureDto>> ListAsync(Guid taskId, string section, string qualifier)
    {
        var scope = Scope(section, qualifier);
        await FindTaskAsync(taskId);
        var rows = await tasks.Context.Queryable<OperationSignature101>()
            .Where(item => item.Scope == scope && item.ScopeId == taskId).ToListAsync();
        return rows.Select(item => new SignatureDto(item.Role, item.SignerName, item.SignedAt)).ToArray();
    }

    public async Task<SignatureDto> SignAsync(Guid taskId, string section, string qualifier, SignRecordInput input)
    {
        var scope = Scope(section, qualifier);
        if (!SignatureRoles.Equipment.Contains(input.Role))
            throw Oops.Oh("不支持的签名角色。").StatusCode(400);
        await FindMutableTaskAsync(taskId);
        var database = tasks.Context;
        if (await database.Queryable<OperationSignature101>().AnyAsync(item =>
                item.Scope == scope && item.ScopeId == taskId && item.Role == input.Role))
            throw Oops.Oh("该角色已签名。").StatusCode(409);
        if (await database.Queryable<OperationSignature101>().AnyAsync(item =>
                item.Scope == scope && item.ScopeId == taskId && item.SignerUserId == currentUser.UserId))
            throw Oops.Oh("当前账号已在此位置签名。").StatusCode(409);
        var signature = new OperationSignature101
        {
            Scope = scope, ScopeId = taskId, Role = input.Role,
            SignerUserId = currentUser.UserId, SignerName = currentUser.RealName, SignedAt = DateTime.UtcNow
        };
        try { await database.Insertable(signature).ExecuteCommandAsync(); }
        catch (PostgresException error) when (error.SqlState == PostgresErrorCodes.UniqueViolation)
        { throw Oops.Oh("该角色或账号已在此位置签名。").StatusCode(409); }
        return new SignatureDto(signature.Role, signature.SignerName, signature.SignedAt);
    }

    public async Task WithdrawAsync(Guid taskId, string section, string qualifier, string role)
    {
        var scope = Scope(section, qualifier);
        await FindMutableTaskAsync(taskId);
        var signature = await tasks.Context.Queryable<OperationSignature101>().FirstAsync(item =>
            item.Scope == scope && item.ScopeId == taskId && item.Role == role)
            ?? throw Oops.Oh("记录不存在。").StatusCode(404);
        if (signature.SignerUserId != currentUser.UserId && !currentUser.IsAdministrator)
            throw Oops.Oh("只能撤回自己的签名。").StatusCode(403);
        await tasks.Context.Deleteable<OperationSignature101>().In(signature.Id).ExecuteCommandAsync();
    }

    private static string Scope(string section, string qualifier)
    {
        if (section == "equipment" && new[] { "0", "1", "2" }.Contains(qualifier))
            return $"equipment:{qualifier}";
        if (section == "transfer" && TransferFieldAllowList101.TableIds.Contains(qualifier))
            return $"transfer:{qualifier}";
        throw Oops.Oh("不支持的签署位置。").StatusCode(400);
    }

    private async Task<Task101> FindTaskAsync(Guid id) =>
        await tasks.GetFirstAsync(item => item.Id == id) ?? throw Oops.Oh("记录不存在。").StatusCode(404);

    private async Task FindMutableTaskAsync(Guid id)
    {
        var task = await FindTaskAsync(id);
        try { TaskWriteGuard.EnsureMutable(task.Status); }
        catch (InvalidOperationException error) { throw Oops.Oh(error.Message).StatusCode(409); }
    }
}

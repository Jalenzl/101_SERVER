using Admin.NET.Application101.Dtos.Common;
using Admin.NET.Application101.Dtos.Tasks;
using Admin.NET.Core101.Domain;

namespace Admin.NET.Application101.Services;

public sealed class TaskService101(SqlSugarRepository<Task101> repository) : ITransient
{
    public async Task<PageResult<TaskDto>> PageAsync(TaskPageQuery input)
    {
        var keyword = input.Keyword?.Trim();
        var query = repository.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(keyword), item =>
                item.Name.Contains(keyword!) || item.Rig.Contains(keyword!) || item.RigCode.Contains(keyword!) ||
                item.EngineModel.Contains(keyword!) || item.TestType.Contains(keyword!) || item.Client.Contains(keyword!))
            .WhereIF(input.Status.HasValue, item => item.Status == input.Status)
            .OrderByDescending(item => item.PlannedDate);
        RefAsync<int> total = 0;
        var items = await query.Select(item => new TaskDto
        {
            Id = item.Id, Name = item.Name, Department = item.Department, Area = item.Area, Rig = item.Rig,
            RigCode = item.RigCode, EngineModel = item.EngineModel, TestType = item.TestType,
            IgnitionDuration = item.IgnitionDuration, IgnitionCount = item.IgnitionCount, Client = item.Client,
            PlannedDate = item.PlannedDate, Status = item.Status
        }).ToPageListAsync(input.Page, input.PageSize, total);
        return Page(items, input, total);
    }

    public async Task<TaskDto> GetAsync(Guid id)
    {
        var item = await repository.AsQueryable().Where(entity => entity.Id == id)
            .Select(entity => new TaskDto
            {
                Id = entity.Id, Name = entity.Name, Department = entity.Department, Area = entity.Area,
                Rig = entity.Rig, RigCode = entity.RigCode, EngineModel = entity.EngineModel,
                TestType = entity.TestType, IgnitionDuration = entity.IgnitionDuration,
                IgnitionCount = entity.IgnitionCount, Client = entity.Client, PlannedDate = entity.PlannedDate,
                Status = entity.Status
            }).FirstAsync();
        return item ?? throw NotFound();
    }

    public async Task<Guid> CreateAsync(CreateTaskInput input)
    {
        var item = Map(input, new Task101());
        await repository.InsertAsync(item);
        return item.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateTaskInput input)
    {
        var item = await FindAsync(id);
        TaskWriteGuard.EnsureMutable(item.Status);
        Map(input, item);
        item.UpdateTime = DateTime.UtcNow;
        await repository.AsUpdateable(item).ExecuteCommandAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var item = await FindAsync(id);
        TaskWriteGuard.EnsureMutable(item.Status);
        var database = repository.Context;
        await database.Ado.BeginTranAsync();
        try
        {
            var operationIds = await database.Queryable<Operation101>()
                .Where(operation => operation.TaskId == id).Select(operation => operation.Id).ToListAsync();
            if (operationIds.Count > 0)
            {
                await database.Deleteable<OperationCheck101>().Where(row => operationIds.Contains(row.OperationId)).ExecuteCommandAsync();
                await database.Deleteable<OperationSignature101>()
                    .Where(row => row.Scope == "operation" && operationIds.Contains(row.ScopeId)).ExecuteCommandAsync();
            }

            await database.Deleteable<Operation101>().Where(row => row.TaskId == id).ExecuteCommandAsync();
            await database.Deleteable<TaskPlan101>().Where(row => row.TaskId == id).ExecuteCommandAsync();
            await database.Deleteable<TaskPerson101>().Where(row => row.TaskId == id).ExecuteCommandAsync();
            await database.Deleteable<TaskDevice101>().Where(row => row.TaskId == id).ExecuteCommandAsync();
            await database.Deleteable<TaskTransferRecord101>().Where(row => row.TaskId == id).ExecuteCommandAsync();
            await database.Deleteable<TaskDocument101>().Where(row => row.TaskId == id).ExecuteCommandAsync();
            await database.Deleteable<TaskWorkflow101>().Where(row => row.TaskId == id).ExecuteCommandAsync();
            item.IsDelete = true;
            item.UpdateTime = DateTime.UtcNow;
            await database.Updateable(item).UpdateColumns(row => new { row.IsDelete, row.UpdateTime }).ExecuteCommandAsync();
            await database.Ado.CommitTranAsync();
        }
        catch
        {
            await database.Ado.RollbackTranAsync();
            throw;
        }
    }

    public async Task SetStatusAsync(Guid id, TaskStatus101 status)
    {
        var item = await FindAsync(id);
        TaskWriteGuard.EnsureMutable(item.Status);
        if (status == TaskStatus101.Completed)
            await EnsureCompletionReadyAsync(id);
        item.Status = status;
        item.UpdateTime = DateTime.UtcNow;
        await repository.AsUpdateable(item).UpdateColumns(row => new { row.Status, row.UpdateTime }).ExecuteCommandAsync();
    }

    private async Task EnsureCompletionReadyAsync(Guid taskId)
    {
        var missing = new List<string>();
        if (!await repository.Context.Queryable<TaskWorkflow101>().AnyAsync(row => row.TaskId == taskId))
            missing.Add("未选择流程");
        if (!await repository.Context.Queryable<Operation101>().AnyAsync(row => row.TaskId == taskId))
            missing.Add("尚未生成操作");
        if (missing.Count > 0)
            throw Oops.Oh("任务不能完成：" + string.Join("；", missing) + "。").StatusCode(409);
    }

    private async Task<Task101> FindAsync(Guid id) =>
        await repository.GetFirstAsync(item => item.Id == id) ?? throw NotFound();

    private static Task101 Map(CreateTaskInput input, Task101 item)
    {
        item.Name = input.Name.Trim(); item.Department = input.Department.Trim(); item.Area = input.Area.Trim();
        item.Rig = input.Rig.Trim(); item.RigCode = input.RigCode.Trim(); item.EngineModel = input.EngineModel.Trim();
        item.TestType = input.TestType.Trim(); item.IgnitionDuration = input.IgnitionDuration;
        item.IgnitionCount = input.IgnitionCount; item.Client = input.Client.Trim();
        item.PlannedDate = input.PlannedDate; item.Status = input.Status;
        return item;
    }

    private static PageResult<TaskDto> Page(IReadOnlyList<TaskDto> items, TaskPageQuery input, int total) =>
        new() { Items = items, Page = input.Page, PageSize = input.PageSize, Total = total };

    private static Exception NotFound() => Oops.Oh("记录不存在。").StatusCode(404);
}

using Admin.NET.Application101.Dtos.Common;
using Admin.NET.Application101.Dtos.Tasks;
using Admin.NET.Application101.Dtos.Operations;
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
        EnsureMutable(item.Status);
        Map(input, item);
        item.UpdateTime = DateTime.UtcNow;
        await repository.AsUpdateable(item).ExecuteCommandAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var item = await FindAsync(id);
        EnsureMutable(item.Status);
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
        EnsureMutable(item.Status);
        if (status == TaskStatus101.Completed)
            await EnsureCompletionReadyAsync(id);
        item.Status = status;
        item.UpdateTime = DateTime.UtcNow;
        await repository.AsUpdateable(item).UpdateColumns(row => new { row.Status, row.UpdateTime }).ExecuteCommandAsync();
    }

    public async Task<TaskWorkflowSelectionDto> GetWorkflowAsync(Guid taskId)
    {
        await FindAsync(taskId);
        var ids = await repository.Context.Queryable<TaskWorkflow101>().Where(item => item.TaskId == taskId)
            .OrderBy(item => item.OrderNo).Select(item => item.WorkflowNodeId).ToListAsync();
        return new TaskWorkflowSelectionDto(ids);
    }

    public async Task SaveWorkflowAsync(Guid taskId, SaveTaskWorkflowInput input)
    {
        var task = await FindAsync(taskId);
        EnsureMutable(task.Status);
        var requested = input.WorkflowNodeIds.Distinct().ToArray();
        var database = repository.Context;
        var available = await database.Queryable<WorkflowNode101>()
            .Where(item => item.Enabled && item.Department == task.Department).ToListAsync();
        var existingOperations = await database.Queryable<Operation101>().Where(item => item.TaskId == taskId).ToListAsync();
        var operationIds = existingOperations.Select(item => item.Id).ToArray();
        var usedOperationIds = new HashSet<Guid>();
        if (operationIds.Length > 0)
        {
            var checkedIds = await database.Queryable<OperationCheck101>()
                .Where(item => operationIds.Contains(item.OperationId)).Select(item => item.OperationId).ToListAsync();
            var signedIds = await database.Queryable<OperationSignature101>()
                .Where(item => item.Scope == "operation" && operationIds.Contains(item.ScopeId))
                .Select(item => item.ScopeId).ToListAsync();
            usedOperationIds.UnionWith(checkedIds);
            usedOperationIds.UnionWith(signedIds);
        }

        OperationPlan plan;
        try
        {
            plan = WorkflowOperationPlanner.Plan(requested, available, existingOperations, usedOperationIds);
        }
        catch (ArgumentException error)
        {
            throw Oops.Oh(error.Message).StatusCode(409);
        }
        if (plan.BlockedOperationIds.Count > 0)
            throw Oops.Oh("工步已有执行数据，不能取消对应流程。").StatusCode(409);

        var existingLinks = await database.Queryable<TaskWorkflow101>().Where(item => item.TaskId == taskId).ToListAsync();
        var linkByNode = existingLinks.ToDictionary(item => item.WorkflowNodeId);
        var linkDelta = SelectionReconciler.Compare(linkByNode.Keys, requested);
        await database.Ado.BeginTranAsync();
        try
        {
            if (plan.RemoveOperationIds.Count > 0)
                await database.Deleteable<Operation101>().In(plan.RemoveOperationIds).ExecuteCommandAsync();
            var removeNodeIds = linkDelta.Remove.ToArray();
            if (removeNodeIds.Length > 0)
                await database.Deleteable<TaskWorkflow101>()
                    .Where(item => item.TaskId == taskId && removeNodeIds.Contains(item.WorkflowNodeId)).ExecuteCommandAsync();
            var orderByNode = requested.Select((id, index) => (id, order: index + 1)).ToDictionary(item => item.id, item => item.order);
            var newLinks = linkDelta.Add.Select(id => new TaskWorkflow101
                { TaskId = taskId, WorkflowNodeId = id, OrderNo = orderByNode[id] }).ToList();
            if (newLinks.Count > 0) await database.Insertable(newLinks).ExecuteCommandAsync();
            var retainedLinks = existingLinks.Where(item => orderByNode.ContainsKey(item.WorkflowNodeId)).ToList();
            foreach (var link in retainedLinks) link.OrderNo = orderByNode[link.WorkflowNodeId];
            if (retainedLinks.Count > 0)
                await database.Updateable(retainedLinks).UpdateColumns(item => new { item.OrderNo }).ExecuteCommandAsync();
            var operations = plan.CreateFrom.Select(node => new Operation101
            {
                TaskId = taskId, WorkflowNodeId = node.Id, Code = $"OP-{node.OrderNo:000}",
                Phase = node.Area, Process = node.Process, Step = node.Step, Post = node.Post,
                OperationDate = task.PlannedDate, Status = OperationStatus101.NotStarted, OrderNo = node.OrderNo
            }).ToList();
            if (operations.Count > 0) await database.Insertable(operations).ExecuteCommandAsync();
            await database.Ado.CommitTranAsync();
        }
        catch
        {
            await database.Ado.RollbackTranAsync();
            throw;
        }
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

    private static void EnsureMutable(TaskStatus101 status)
    {
        try
        {
            TaskWriteGuard.EnsureMutable(status);
        }
        catch (InvalidOperationException error)
        {
            throw Oops.Oh(error.Message).StatusCode(409);
        }
    }
}

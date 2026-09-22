using System.Text.Json;
using Admin.NET.Application101.Dtos.Preparation;
using Admin.NET.Application101.Validation;
using Admin.NET.Core101.Domain;

namespace Admin.NET.Application101.Services;

public sealed class TaskPreparationService101(SqlSugarRepository<Task101> tasks) : ITransient
{
    public async Task<TaskPlanDto> GetPlanAsync(Guid taskId)
    {
        var task = await FindTaskAsync(taskId);
        var entities = await tasks.Context.Queryable<TaskPlan101>().Where(item => item.TaskId == taskId)
            .OrderBy(item => item.OrderNo).ToListAsync();
        var rows = entities.Select(item => new TaskPlanRowDto(item.Id, item.CompletedDate,
            item.Content, item.OwnerUnit, item.SupportUnit, item.Remark, item.OrderNo)).ToArray();
        return new TaskPlanDto(taskId, task.IgnitionTime, rows);
    }

    public async Task SavePlanAsync(Guid taskId, SaveTaskPlanInput input)
    {
        var task = await FindMutableTaskAsync(taskId);
        var database = tasks.Context;
        await database.Ado.BeginTranAsync();
        try
        {
            await database.Deleteable<TaskPlan101>().Where(item => item.TaskId == taskId).ExecuteCommandAsync();
            var rows = input.Rows.OrderBy(item => item.Order).Select(item => new TaskPlan101
            {
                TaskId = taskId, CompletedDate = item.CompletedDate, Content = item.Content.Trim(),
                OwnerUnit = item.OwnerUnit.Trim(), SupportUnit = item.SupportUnit.Trim(),
                Remark = item.Remark.Trim(), OrderNo = item.Order
            }).ToList();
            if (rows.Count > 0) await database.Insertable(rows).ExecuteCommandAsync();
            task.IgnitionTime = input.IgnitionTime; task.UpdateTime = DateTime.UtcNow;
            await database.Updateable(task).UpdateColumns(item => new { item.IgnitionTime, item.UpdateTime }).ExecuteCommandAsync();
            await database.Ado.CommitTranAsync();
        }
        catch
        {
            await database.Ado.RollbackTranAsync();
            throw;
        }
    }

    public async Task<TaskPreparationDto> GetPreparationAsync(Guid taskId)
    {
        await FindTaskAsync(taskId);
        var database = tasks.Context;
        var personnelEntities = await database.Queryable<TaskPerson101>().Where(item => item.TaskId == taskId)
            .OrderBy(item => item.OrderNo).ToListAsync();
        var deviceEntities = await database.Queryable<TaskDevice101>().Where(item => item.TaskId == taskId)
            .OrderBy(item => item.OrderNo).ToListAsync();
        var documentEntities = await database.Queryable<TaskDocument101>().Where(item => item.TaskId == taskId)
            .OrderBy(item => item.OrderNo).ToListAsync();
        var personnel = personnelEntities.Select(item => new TaskPersonnelDto(item.Id, item.PersonId,
            item.Kind, item.Role, item.System, item.Rig, item.PostName, item.PostCode, item.OrderNo)).ToArray();
        var devices = deviceEntities.Select(item => new TaskDeviceDto(item.Id, item.DeviceId,
            item.System, item.OrderNo)).ToArray();
        var documents = documentEntities.Select(item => new TaskDocumentDto(item.Id, item.DocumentId,
            item.DocumentType, item.OrderNo)).ToArray();
        return new TaskPreparationDto(personnel, devices, documents);
    }

    public async Task SavePersonnelAsync(Guid taskId, SaveTaskPersonnelInput input)
    {
        await FindMutableTaskAsync(taskId);
        var requested = input.Team.Select(item => new TaskPerson101
        {
            TaskId = taskId, PersonId = item.PersonId, Kind = "团队", Role = item.Role.Trim(), OrderNo = item.Order
        }).Concat(input.Posts.Select(item => new TaskPerson101
        {
            TaskId = taskId, PersonId = item.PersonId, Kind = "岗位", Role = "岗位人员", System = item.System,
            Rig = item.Rig.Trim(), PostName = item.PostName.Trim(), PostCode = item.PostCode.Trim(), OrderNo = item.Order
        })).ToArray();
        var personIds = requested.Select(item => item.PersonId).Distinct().ToArray();
        await EnsureSourcesExist(personIds,
            () => tasks.Context.Queryable<Person101>().Where(item => personIds.Contains(item.Id)).CountAsync(), "人员");
        await ReconcileAsync(taskId, requested, Key,
            () => tasks.Context.Queryable<TaskPerson101>().Where(item => item.TaskId == taskId).ToListAsync());
    }

    public async Task SaveDevicesAsync(Guid taskId, SaveTaskDevicesInput input)
    {
        await FindMutableTaskAsync(taskId);
        var requested = input.Systems.SelectMany(group => group.DeviceIds.Select((id, index) => new TaskDevice101
        {
            TaskId = taskId, DeviceId = id, System = group.System, OrderNo = index + 1
        })).ToArray();
        var deviceIds = requested.Select(item => item.DeviceId).Distinct().ToArray();
        await EnsureSourcesExist(deviceIds,
            () => tasks.Context.Queryable<Device101>().Where(item => deviceIds.Contains(item.Id)).CountAsync(), "设备");
        await ReconcileAsync(taskId, requested, item => $"{item.DeviceId:N}|{item.System}",
            () => tasks.Context.Queryable<TaskDevice101>().Where(item => item.TaskId == taskId).ToListAsync());
    }

    public async Task SaveDocumentsAsync(Guid taskId, SaveTaskDocumentsInput input)
    {
        await FindMutableTaskAsync(taskId);
        var requested = input.Types.SelectMany(group => group.DocumentIds.Select((id, index) => new TaskDocument101
        {
            TaskId = taskId, DocumentId = id, DocumentType = group.DocumentType.Trim(), OrderNo = index + 1
        })).ToArray();
        var documentIds = requested.Select(item => item.DocumentId).Distinct().ToArray();
        await EnsureSourcesExist(documentIds,
            () => tasks.Context.Queryable<Document101>().Where(item => documentIds.Contains(item.Id)).CountAsync(), "文件");
        await ReconcileAsync(taskId, requested, item => $"{item.DocumentId:N}|{item.DocumentType}",
            () => tasks.Context.Queryable<TaskDocument101>().Where(item => item.TaskId == taskId).ToListAsync());
    }

    public async Task<IReadOnlyList<TransferRowDto>> GetTransfersAsync(Guid taskId, string tableId)
    {
        await FindTaskAsync(taskId);
        if (!TransferFieldAllowList101.TableIds.Contains(tableId))
            throw Oops.Oh("不支持的传递表。").StatusCode(400);
        var rows = await tasks.Context.Queryable<TaskTransferRecord101>()
            .Where(item => item.TaskId == taskId && item.TableId == tableId).OrderBy(item => item.OrderNo).ToListAsync();
        return rows.Select(item => new TransferRowDto(item.Id, item.OrderNo,
            JsonDocument.Parse(item.DataJson).RootElement.Clone())).ToArray();
    }

    public async Task SaveTransfersAsync(Guid taskId, string tableId, SaveTransferInput input)
    {
        await FindMutableTaskAsync(taskId);
        foreach (var row in input.Rows) TransferFieldAllowList101.Validate(tableId, row.Data);
        if (input.Rows.Select(item => item.Id).Distinct().Count() != input.Rows.Count)
            throw Oops.Oh("传递表行 ID 不能重复。").StatusCode(400);
        var database = tasks.Context;
        await database.Ado.BeginTranAsync();
        try
        {
            await database.Deleteable<TaskTransferRecord101>()
                .Where(item => item.TaskId == taskId && item.TableId == tableId).ExecuteCommandAsync();
            var rows = input.Rows.OrderBy(item => item.Order).Select(item => new TaskTransferRecord101
            {
                Id = item.Id, TaskId = taskId, TableId = tableId, DataJson = item.Data.GetRawText(), OrderNo = item.Order
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

    private async Task ReconcileAsync<TEntity>(Guid taskId, IReadOnlyCollection<TEntity> requested,
        Func<TEntity, string> key, Func<Task<List<TEntity>>> loadExisting) where TEntity : Entity101Base, new()
    {
        var existing = await loadExisting();
        var requestedByKey = requested.GroupBy(key).ToDictionary(group => group.Key, group => group.Last());
        var existingByKey = existing.ToDictionary(key);
        var delta = SelectionReconciler.Compare(existingByKey.Keys, requestedByKey.Keys);
        var database = tasks.Context;
        await database.Ado.BeginTranAsync();
        try
        {
            var removeIds = delta.Remove.Select(item => existingByKey[item].Id).ToArray();
            if (removeIds.Length > 0) await database.Deleteable<TEntity>().In(removeIds).ExecuteCommandAsync();
            var additions = delta.Add.Select(item => requestedByKey[item]).ToList();
            if (additions.Count > 0) await database.Insertable(additions).ExecuteCommandAsync();
            var retained = requestedByKey.Keys.Intersect(existingByKey.Keys).Select(item =>
            {
                var update = requestedByKey[item];
                var original = existingByKey[item];
                update.Id = original.Id;
                update.CreateTime = original.CreateTime;
                update.CreateUserId = original.CreateUserId;
                update.CreateUserName = original.CreateUserName;
                update.UpdateTime = DateTime.UtcNow;
                return update;
            }).ToList();
            if (retained.Count > 0) await database.Updateable(retained).ExecuteCommandAsync();
            await database.Ado.CommitTranAsync();
        }
        catch
        {
            await database.Ado.RollbackTranAsync();
            throw;
        }
    }

    private static async Task EnsureSourcesExist(IReadOnlyCollection<Guid> ids,
        Func<Task<int>> countExisting, string label)
    {
        if (ids.Count == 0) return;
        if (await countExisting() != ids.Count) throw Oops.Oh($"存在无效的{label}引用。").StatusCode(409);
    }

    private async Task<Task101> FindTaskAsync(Guid taskId) =>
        await tasks.GetFirstAsync(item => item.Id == taskId) ?? throw Oops.Oh("记录不存在。").StatusCode(404);

    private async Task<Task101> FindMutableTaskAsync(Guid taskId)
    {
        var task = await FindTaskAsync(taskId);
        try
        {
            TaskWriteGuard.EnsureMutable(task.Status);
        }
        catch (InvalidOperationException error)
        {
            throw Oops.Oh(error.Message).StatusCode(409);
        }
        return task;
    }

    private static string Key(TaskPerson101 item) =>
        $"{item.PersonId:N}|{item.Kind}|{item.Role}|{item.System?.ToString() ?? string.Empty}";
}

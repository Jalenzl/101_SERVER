using Admin.NET.Application101.Dtos.Records;
using Admin.NET.Core101.Domain;
using Newtonsoft.Json.Linq;

namespace Admin.NET.Application101.Services;

public sealed class TaskRecordService101(SqlSugarRepository<Task101> tasks) : ITransient
{
    public static readonly IReadOnlySet<string> Kinds = new HashSet<string>(StringComparer.Ordinal)
    {
        "fmeca", "fmea", "task-risk", "summary-overview", "summary-execution",
        "summary-data", "summary-medium", "stops"
    };

    public async Task<IReadOnlyList<RecordRowDto>> GetAsync(Guid taskId, string kind)
    {
        ValidateKind(kind);
        await FindTaskAsync(taskId);
        var rows = await tasks.Context.Queryable<TaskRecord101>()
            .Where(item => item.TaskId == taskId && item.Kind == kind)
            .OrderBy(item => item.OrderNo).ToListAsync();
        return rows.Select(item => new RecordRowDto(item.Id, item.OrderNo, item.DataJson)).ToArray();
    }

    public async Task SaveAsync(Guid taskId, string kind, SaveRecordRowsInput input)
    {
        ValidateKind(kind);
        var task = await FindTaskAsync(taskId);
        if (!kind.StartsWith("fm", StringComparison.Ordinal) && kind != "task-risk")
            EnsureMutable(task.Status);
        if (input.Rows is null || input.Rows.Select(item => item.Id).Distinct().Count() != input.Rows.Count ||
            input.Rows.Any(item => item.Id == Guid.Empty || item.Order < 0 ||
                item.Data is null || item.Data.ToString(Newtonsoft.Json.Formatting.None).Length > 65536 ||
                (kind == "stops" && !HasRequiredStopVerification(item.Data))))
            throw Oops.Oh("记录格式无效。").StatusCode(400);

        var database = tasks.Context;
        await database.Ado.BeginTranAsync();
        try
        {
            await database.Deleteable<TaskRecord101>().Where(item => item.TaskId == taskId && item.Kind == kind)
                .ExecuteCommandAsync();
            var rows = input.Rows.OrderBy(item => item.Order).Select(item => new TaskRecord101
            {
                Id = item.Id, TaskId = taskId, Kind = kind, OrderNo = item.Order,
                DataJson = (JObject)item.Data.DeepClone()
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

    private async Task<Task101> FindTaskAsync(Guid id) =>
        await tasks.GetFirstAsync(item => item.Id == id) ?? throw Oops.Oh("记录不存在。").StatusCode(404);

    private static void ValidateKind(string kind)
    {
        if (!Kinds.Contains(kind)) throw Oops.Oh("不支持的任务记录类型。").StatusCode(400);
    }

    public static bool HasRequiredStopVerification(JObject data)
    {
        if (data is null) return false;
        var restored = data["restoredAt"]?.Type == JTokenType.String &&
            !string.IsNullOrWhiteSpace(data.Value<string>("restoredAt"));
        return !restored || data["verification"]?.Type == JTokenType.String &&
            !string.IsNullOrWhiteSpace(data.Value<string>("verification"));
    }

    private static void EnsureMutable(TaskStatus101 status)
    {
        try { TaskWriteGuard.EnsureMutable(status); }
        catch (InvalidOperationException error) { throw Oops.Oh(error.Message).StatusCode(409); }
    }
}

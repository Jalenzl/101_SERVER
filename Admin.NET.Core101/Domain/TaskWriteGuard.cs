using Admin.NET.Core101.Enum;

namespace Admin.NET.Core101.Domain;

public static class TaskWriteGuard
{
    public static void EnsureMutable(TaskStatus101 status)
    {
        if (status == TaskStatus101.Completed)
            throw new InvalidOperationException("任务已完成，不允许修改。");
    }

    public static void EnsureInitialStatus(TaskStatus101 status)
    {
        if (status != TaskStatus101.InProgress)
            throw new InvalidOperationException("新任务只能是进行中状态。");
    }

    public static void EnsureMetadataUpdateDoesNotChangeStatus(TaskStatus101 current, TaskStatus101 requested)
    {
        if (current != requested)
            throw new InvalidOperationException("请通过任务状态接口修改状态。");
    }

    public static void EnsureDeletable(bool hasSignatures)
    {
        if (hasSignatures)
            throw new InvalidOperationException("任务已有签名，不能删除。");
    }
}

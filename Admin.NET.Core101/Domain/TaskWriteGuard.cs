using Admin.NET.Core101.Enum;

namespace Admin.NET.Core101.Domain;

public static class TaskWriteGuard
{
    public static void EnsureMutable(TaskStatus101 status)
    {
        if (status == TaskStatus101.Completed)
            throw new InvalidOperationException("任务已完成，不允许修改。");
    }
}

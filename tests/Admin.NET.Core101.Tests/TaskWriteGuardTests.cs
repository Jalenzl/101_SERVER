using Admin.NET.Core101.Domain;
using Admin.NET.Core101.Enum;

namespace Admin.NET.Core101.Tests;

public sealed class TaskWriteGuardTests
{
    [Theory]
    [InlineData(TaskStatus101.InProgress)]
    [InlineData(TaskStatus101.Terminated)]
    public void EnsureMutable_AllowsNonCompletedTasks(TaskStatus101 status)
    {
        TaskWriteGuard.EnsureMutable(status);
    }

    [Fact]
    public void EnsureMutable_RejectsCompletedTask()
    {
        var error = Assert.Throws<InvalidOperationException>(
            () => TaskWriteGuard.EnsureMutable(TaskStatus101.Completed));
        Assert.Equal("任务已完成，不允许修改。", error.Message);
    }

    [Fact]
    public void EnsureInitialStatus_RejectsCompletedTask()
    {
        Assert.Throws<InvalidOperationException>(() => TaskWriteGuard.EnsureInitialStatus(TaskStatus101.Completed));
    }

    [Fact]
    public void EnsureMetadataUpdateDoesNotChangeStatus_RejectsChange()
    {
        Assert.Throws<InvalidOperationException>(() =>
            TaskWriteGuard.EnsureMetadataUpdateDoesNotChangeStatus(
                TaskStatus101.InProgress, TaskStatus101.Completed));
    }

    [Fact]
    public void EnsureDeletable_RejectsSignedTask()
    {
        Assert.Throws<InvalidOperationException>(() => TaskWriteGuard.EnsureDeletable(true));
        TaskWriteGuard.EnsureDeletable(false);
    }
}

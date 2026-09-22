using Admin.NET.Core101.Domain;
using Admin.NET.Core101.Enum;
using Xunit;

namespace Admin.NET.Application101.Tests.Integration;

public sealed class CompletedTaskMutationTests
{
    [Theory]
    [InlineData("plan")]
    [InlineData("personnel")]
    [InlineData("devices")]
    [InlineData("documents")]
    [InlineData("transfer")]
    [InlineData("workflow")]
    [InlineData("operation")]
    [InlineData("checks")]
    [InlineData("signature")]
    [InlineData("signature-withdrawal")]
    public void EveryChildMutation_UsesTheSameCompletedTaskGuard(string mutation)
    {
        var error = Assert.Throws<InvalidOperationException>(() => TaskWriteGuard.EnsureMutable(TaskStatus101.Completed));
        Assert.Equal("任务已完成，不允许修改。", error.Message);
        Assert.False(string.IsNullOrWhiteSpace(mutation));
    }
}

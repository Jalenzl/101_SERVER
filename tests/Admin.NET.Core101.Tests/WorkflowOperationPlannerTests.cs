using Admin.NET.Core101.Domain;
using Admin.NET.Core101.Entity;
using Admin.NET.Core101.Enum;

namespace Admin.NET.Core101.Tests;

public sealed class WorkflowOperationPlannerTests
{
    private static readonly WorkflowNode101 Node1 = new() { Id = Guid.Parse("20000000-0000-0000-0000-000000000001") };
    private static readonly WorkflowNode101 Node2 = new() { Id = Guid.Parse("20000000-0000-0000-0000-000000000002") };

    [Fact]
    public void Plan_IsIdempotent_AndCreatesOnlyNewSelections()
    {
        var operation = new Operation101 { Id = Guid.NewGuid(), WorkflowNodeId = Node1.Id, Status = OperationStatus101.NotStarted };
        var plan = WorkflowOperationPlanner.Plan(new[] { Node1.Id, Node2.Id }, new[] { Node1, Node2 }, new[] { operation }, Array.Empty<Guid>());
        Assert.Equal(new[] { Node2.Id }, plan.CreateFrom.Select(item => item.Id));
        Assert.Empty(plan.RemoveOperationIds);
        Assert.Empty(plan.BlockedOperationIds);
    }

    [Fact]
    public void Plan_RemovesUntouchedOperation_ButBlocksExecutedOperation()
    {
        var removable = new Operation101 { Id = Guid.NewGuid(), WorkflowNodeId = Node1.Id, Status = OperationStatus101.NotStarted };
        var blocked = new Operation101 { Id = Guid.NewGuid(), WorkflowNodeId = Node2.Id, Status = OperationStatus101.InProgress };
        var plan = WorkflowOperationPlanner.Plan(Array.Empty<Guid>(), new[] { Node1, Node2 },
            new[] { removable, blocked }, new[] { blocked.Id });
        Assert.Equal(new[] { removable.Id }, plan.RemoveOperationIds);
        Assert.Equal(new[] { blocked.Id }, plan.BlockedOperationIds);
    }

    [Fact]
    public void Plan_RejectsUnavailableNode()
    {
        Assert.Throws<ArgumentException>(() => WorkflowOperationPlanner.Plan(
            new[] { Guid.NewGuid() }, new[] { Node1 }, Array.Empty<Operation101>(), Array.Empty<Guid>()));
    }
}

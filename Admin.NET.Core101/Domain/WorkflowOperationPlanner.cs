using Admin.NET.Core101.Entity;
using Admin.NET.Core101.Enum;

namespace Admin.NET.Core101.Domain;

public sealed record OperationPlan(
    IReadOnlyList<WorkflowNode101> CreateFrom,
    IReadOnlyList<Guid> RemoveOperationIds,
    IReadOnlyList<Guid> BlockedOperationIds);

public static class WorkflowOperationPlanner
{
    public static OperationPlan Plan(
        IReadOnlyCollection<Guid> requestedNodeIds,
        IReadOnlyCollection<WorkflowNode101> availableNodes,
        IReadOnlyCollection<Operation101> existingOperations,
        IReadOnlyCollection<Guid> operationIdsWithChecksOrSignatures)
    {
        var requested = requestedNodeIds.ToHashSet();
        var available = availableNodes.ToDictionary(item => item.Id);
        if (requested.Any(id => !available.ContainsKey(id)))
            throw new ArgumentException("包含不可用的流程节点。", nameof(requestedNodeIds));

        var existingByNode = existingOperations.ToDictionary(item => item.WorkflowNodeId);
        var executed = operationIdsWithChecksOrSignatures.ToHashSet();
        var create = requested.Where(id => !existingByNode.ContainsKey(id)).Select(id => available[id])
            .OrderBy(item => item.OrderNo).ToArray();
        var deselected = existingOperations.Where(item => !requested.Contains(item.WorkflowNodeId)).ToArray();
        var blocked = deselected.Where(item => item.Status != OperationStatus101.NotStarted || executed.Contains(item.Id))
            .Select(item => item.Id).OrderBy(item => item).ToArray();
        var remove = deselected.Where(item => item.Status == OperationStatus101.NotStarted && !executed.Contains(item.Id))
            .Select(item => item.Id).OrderBy(item => item).ToArray();
        return new OperationPlan(create, remove, blocked);
    }
}

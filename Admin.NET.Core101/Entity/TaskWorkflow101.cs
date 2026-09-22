namespace Admin.NET.Core101.Entity;

[SugarTable("t101_task_workflow")]
[SugarIndex("ux_t101_task_workflow", nameof(TaskId), OrderByType.Asc, nameof(WorkflowNodeId), OrderByType.Asc, true)]
public sealed class TaskWorkflow101 : Entity101Base
{
    public Guid TaskId { get; set; }
    public Guid WorkflowNodeId { get; set; }
    public int OrderNo { get; set; }
}

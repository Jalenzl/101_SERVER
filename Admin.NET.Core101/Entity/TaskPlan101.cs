namespace Admin.NET.Core101.Entity;

[SugarTable("t101_task_plan")]
public sealed class TaskPlan101 : Entity101Base
{
    public Guid TaskId { get; set; }
    public DateOnly? CompletedDate { get; set; }
    [SugarColumn(Length = 512)] public string Content { get; set; } = string.Empty;
    [SugarColumn(Length = 128)] public string OwnerUnit { get; set; } = string.Empty;
    [SugarColumn(Length = 128)] public string SupportUnit { get; set; } = string.Empty;
    [SugarColumn(Length = 512)] public string Remark { get; set; } = string.Empty;
    public int OrderNo { get; set; }
}

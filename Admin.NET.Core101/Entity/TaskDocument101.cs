namespace Admin.NET.Core101.Entity;

[SugarTable("t101_task_document")]
[SugarIndex("ux_t101_task_document", nameof(TaskId), OrderByType.Asc, nameof(DocumentId), OrderByType.Asc,
    nameof(DocumentType), OrderByType.Asc, true)]
public sealed class TaskDocument101 : Entity101Base
{
    public Guid TaskId { get; set; }
    public Guid DocumentId { get; set; }
    [SugarColumn(Length = 64)] public string DocumentType { get; set; } = string.Empty;
    public int OrderNo { get; set; }
}

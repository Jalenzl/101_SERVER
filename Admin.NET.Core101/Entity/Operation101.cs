using Admin.NET.Core101.Enum;

namespace Admin.NET.Core101.Entity;

[SugarTable("t101_operation")]
public sealed class Operation101 : Entity101Base
{
    public Guid TaskId { get; set; }
    public Guid WorkflowNodeId { get; set; }
    [SugarColumn(Length = 64)] public string Code { get; set; } = string.Empty;
    [SugarColumn(Length = 64)] public string Phase { get; set; } = string.Empty;
    [SugarColumn(Length = 128)] public string Process { get; set; } = string.Empty;
    [SugarColumn(Length = 128)] public string Step { get; set; } = string.Empty;
    [SugarColumn(Length = 64)] public string Post { get; set; } = string.Empty;
    public DateOnly? OperationDate { get; set; }
    [SugarColumn(Length = 1024)] public string QualityRequirement { get; set; } = string.Empty;
    [SugarColumn(Length = 1024)] public string OperationRequirement { get; set; } = string.Empty;
    [SugarColumn(Length = 1024)] public string Attention { get; set; } = string.Empty;
    public OperationStatus101 Status { get; set; }
    public int OrderNo { get; set; }
}

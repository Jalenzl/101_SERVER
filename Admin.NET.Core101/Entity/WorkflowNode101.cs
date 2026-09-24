namespace Admin.NET.Core101.Entity;

[SugarTable("t101_workflow_node")]
public sealed class WorkflowNode101 : Entity101Base
{
    [SugarColumn(Length = 64)] public string Department { get; set; } = string.Empty;
    [SugarColumn(Length = 64)] public string Area { get; set; } = string.Empty;
    [SugarColumn(Length = 128)] public string Process { get; set; } = string.Empty;
    [SugarColumn(Length = 128)] public string Step { get; set; } = string.Empty;
    [SugarColumn(Length = 64)] public string Post { get; set; } = string.Empty;
    [SugarColumn(Length = 64, IsNullable = true)] public string? CheckPost { get; set; } = string.Empty;
    [SugarColumn(Length = 64, IsNullable = true)] public string? Countersign { get; set; } = string.Empty;
    [SugarColumn(Length = 64, IsNullable = true)] public string? Confirmer { get; set; } = string.Empty;
    [SugarColumn(Length = 512, IsNullable = true)] public string? Remark { get; set; } = string.Empty;
    public int OrderNo { get; set; }
    public bool Enabled { get; set; } = true;
}

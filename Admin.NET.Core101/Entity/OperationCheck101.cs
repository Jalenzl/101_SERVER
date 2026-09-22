namespace Admin.NET.Core101.Entity;

[SugarTable("t101_operation_check")]
[SugarIndex("ux_t101_operation_check", nameof(OperationId), OrderByType.Asc, nameof(OrderNo), OrderByType.Asc, true)]
public sealed class OperationCheck101 : Entity101Base
{
    public Guid OperationId { get; set; }
    [SugarColumn(Length = 256)] public string Item { get; set; } = string.Empty;
    [SugarColumn(Length = 512)] public string Requirement { get; set; } = string.Empty;
    [SugarColumn(Length = 512)] public string Actual { get; set; } = string.Empty;
    [SugarColumn(Length = 512)] public string Remark { get; set; } = string.Empty;
    public int OrderNo { get; set; }
}

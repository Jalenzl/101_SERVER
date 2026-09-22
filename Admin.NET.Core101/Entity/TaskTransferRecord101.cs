using Newtonsoft.Json.Linq;

namespace Admin.NET.Core101.Entity;

[SugarTable("t101_task_transfer_record")]
[SugarIndex("ux_t101_task_transfer", nameof(TaskId), OrderByType.Asc, nameof(TableId), OrderByType.Asc,
    nameof(OrderNo), OrderByType.Asc, true)]
public sealed class TaskTransferRecord101 : Entity101Base
{
    public Guid TaskId { get; set; }
    [SugarColumn(Length = 64)] public string TableId { get; set; } = string.Empty;
    [SugarColumn(IsJson = true, ColumnDataType = "jsonb")]
    public JObject DataJson { get; set; } = new();
    public int OrderNo { get; set; }
}

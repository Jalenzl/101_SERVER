using Newtonsoft.Json.Linq;

namespace Admin.NET.Core101.Entity;

[SugarTable("t101_task_record")]
[SugarIndex("ux_t101_task_record", nameof(TaskId), OrderByType.Asc, nameof(Kind), OrderByType.Asc,
    nameof(Id), OrderByType.Asc, true)]
public sealed class TaskRecord101 : Entity101Base
{
    public Guid TaskId { get; set; }
    [SugarColumn(Length = 32)] public string Kind { get; set; } = string.Empty;
    [SugarColumn(IsJson = true, ColumnDataType = "jsonb")]
    public JObject DataJson { get; set; } = new();
    public int OrderNo { get; set; }
}

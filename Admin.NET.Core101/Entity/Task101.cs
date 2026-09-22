using Admin.NET.Core101.Enum;

namespace Admin.NET.Core101.Entity;

[SugarTable("t101_task")]
public sealed class Task101 : Entity101Base
{
    [SugarColumn(Length = 128)] public string Name { get; set; } = string.Empty;
    [SugarColumn(Length = 64)] public string Department { get; set; } = string.Empty;
    [SugarColumn(Length = 64)] public string Area { get; set; } = string.Empty;
    [SugarColumn(Length = 64)] public string Rig { get; set; } = string.Empty;
    [SugarColumn(Length = 32)] public string RigCode { get; set; } = string.Empty;
    [SugarColumn(Length = 64)] public string EngineModel { get; set; } = string.Empty;
    [SugarColumn(Length = 64)] public string TestType { get; set; } = string.Empty;
    public int IgnitionDuration { get; set; }
    public int IgnitionCount { get; set; }
    [SugarColumn(Length = 128)] public string Client { get; set; } = string.Empty;
    public DateOnly PlannedDate { get; set; }
    public DateTime? IgnitionTime { get; set; }
    public TaskStatus101 Status { get; set; }
}

using Admin.NET.Core101.Enum;

namespace Admin.NET.Core101.Entity;

[SugarTable("t101_task_person")]
[SugarIndex("ux_t101_task_person", nameof(TaskId), OrderByType.Asc, nameof(PersonId), OrderByType.Asc,
    nameof(Kind), OrderByType.Asc, nameof(Role), OrderByType.Asc, nameof(System), OrderByType.Asc, true)]
public sealed class TaskPerson101 : Entity101Base
{
    public Guid TaskId { get; set; }
    public Guid PersonId { get; set; }
    [SugarColumn(Length = 32)] public string Kind { get; set; } = string.Empty;
    [SugarColumn(Length = 64)] public string Role { get; set; } = string.Empty;
    public SystemType101? System { get; set; }
    [SugarColumn(Length = 64)] public string Rig { get; set; } = string.Empty;
    [SugarColumn(Length = 64)] public string PostName { get; set; } = string.Empty;
    [SugarColumn(Length = 32)] public string PostCode { get; set; } = string.Empty;
    public int OrderNo { get; set; }
}

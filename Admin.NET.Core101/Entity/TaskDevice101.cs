using Admin.NET.Core101.Enum;

namespace Admin.NET.Core101.Entity;

[SugarTable("t101_task_device")]
[SugarIndex("ux_t101_task_device", nameof(TaskId), OrderByType.Asc, nameof(DeviceId), OrderByType.Asc,
    nameof(System), OrderByType.Asc, true)]
public sealed class TaskDevice101 : Entity101Base
{
    public Guid TaskId { get; set; }
    public Guid DeviceId { get; set; }
    public SystemType101 System { get; set; }
    public int OrderNo { get; set; }
}

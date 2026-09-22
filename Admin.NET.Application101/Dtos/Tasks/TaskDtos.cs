using Admin.NET.Application101.Dtos.Common;

namespace Admin.NET.Application101.Dtos.Tasks;

public sealed class TaskPageQuery : PageQuery
{
    public TaskStatus101? Status { get; set; }
}

public sealed class TaskDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Rig { get; set; } = string.Empty;
    public string RigCode { get; set; } = string.Empty;
    public string EngineModel { get; set; } = string.Empty;
    public string TestType { get; set; } = string.Empty;
    public int IgnitionDuration { get; set; }
    public int IgnitionCount { get; set; }
    public string Client { get; set; } = string.Empty;
    public DateOnly PlannedDate { get; set; }
    public TaskStatus101 Status { get; set; }
}

public class CreateTaskInput
{
    [Required, RegularExpression(@".*\S.*")]
    public string Name { get; set; } = string.Empty;
    [Required, RegularExpression(@".*\S.*")] public string Department { get; set; } = string.Empty;
    [Required, RegularExpression(@".*\S.*")] public string Area { get; set; } = string.Empty;
    [Required, RegularExpression(@".*\S.*")] public string Rig { get; set; } = string.Empty;
    [Required, RegularExpression(@".*\S.*")] public string RigCode { get; set; } = string.Empty;
    [Required, RegularExpression(@".*\S.*")] public string EngineModel { get; set; } = string.Empty;
    [Required, RegularExpression(@".*\S.*")] public string TestType { get; set; } = string.Empty;
    [Range(0, int.MaxValue)] public int IgnitionDuration { get; set; }
    [Range(0, int.MaxValue)] public int IgnitionCount { get; set; }
    [Required, RegularExpression(@".*\S.*")] public string Client { get; set; } = string.Empty;
    public DateOnly PlannedDate { get; set; }
    public TaskStatus101 Status { get; set; } = TaskStatus101.InProgress;
}

public sealed class UpdateTaskInput : CreateTaskInput;

public sealed class ChangeTaskStatusInput
{
    public TaskStatus101 Status { get; set; }
}

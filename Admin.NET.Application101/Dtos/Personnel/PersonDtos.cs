using Admin.NET.Application101.Dtos.Common;

namespace Admin.NET.Application101.Dtos.Personnel;

public sealed class PersonPageQuery : PageQuery;

public sealed class PersonDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool SpecialOps { get; set; }
    public DateOnly? SpecialOpsValidUntil { get; set; }
    public bool Inspector { get; set; }
    public DateOnly? InspectorValidUntil { get; set; }
    public bool Calibrator { get; set; }
    public DateOnly? CalibratorValidUntil { get; set; }
    public bool ProductAssurance { get; set; }
    public string Contact { get; set; } = string.Empty;
    public int TestCount { get; set; }
    public bool ExamPassed { get; set; }
}

public class CreatePersonInput
{
    [Required, RegularExpression(@".*\S.*")] public string Name { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool SpecialOps { get; set; }
    public DateOnly? SpecialOpsValidUntil { get; set; }
    public bool Inspector { get; set; }
    public DateOnly? InspectorValidUntil { get; set; }
    public bool Calibrator { get; set; }
    public DateOnly? CalibratorValidUntil { get; set; }
    public bool ProductAssurance { get; set; }
    public string Contact { get; set; } = string.Empty;
    [Range(0, int.MaxValue)] public int TestCount { get; set; }
    public bool ExamPassed { get; set; }
}

public sealed class UpdatePersonInput : CreatePersonInput;

namespace Admin.NET.Core101.Entity;

[SugarTable("t101_person")]
public sealed class Person101 : Entity101Base
{
    [SugarColumn(Length = 64)] public string Name { get; set; } = string.Empty;
    [SugarColumn(Length = 16)] public string Gender { get; set; } = string.Empty;
    [SugarColumn(Length = 64)] public string Department { get; set; } = string.Empty;
    [SugarColumn(Length = 64)] public string Area { get; set; } = string.Empty;
    [SugarColumn(Length = 64)] public string Title { get; set; } = string.Empty;
    public bool SpecialOps { get; set; }
    [SugarColumn(IsNullable = true)] public DateOnly? SpecialOpsValidUntil { get; set; }
    public bool Inspector { get; set; }
    [SugarColumn(IsNullable = true)] public DateOnly? InspectorValidUntil { get; set; }
    public bool Calibrator { get; set; }
    [SugarColumn(IsNullable = true)] public DateOnly? CalibratorValidUntil { get; set; }
    public bool ProductAssurance { get; set; }
    [SugarColumn(Length = 64)] public string Contact { get; set; } = string.Empty;
    public int TestCount { get; set; }
    public bool ExamPassed { get; set; }
}

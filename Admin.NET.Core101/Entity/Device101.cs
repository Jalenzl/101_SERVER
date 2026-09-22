using Admin.NET.Core101.Enum;

namespace Admin.NET.Core101.Entity;

[SugarTable("t101_device")]
[SugarIndex("ux_t101_device_code", nameof(Code), OrderByType.Asc, true)]
public sealed class Device101 : Entity101Base
{
    [SugarColumn(Length = 64)] public string Code { get; set; } = string.Empty;
    [SugarColumn(Length = 128)] public string Name { get; set; } = string.Empty;
    [SugarColumn(Length = 64)] public string FactoryCode { get; set; } = string.Empty;
    [SugarColumn(Length = 128)] public string Manufacturer { get; set; } = string.Empty;
    [SugarColumn(Length = 64)] public string Model { get; set; } = string.Empty;
    [SugarColumn(Length = 128)] public string Range { get; set; } = string.Empty;
    [SugarColumn(IsNullable = true)] public DateOnly? EnabledDate { get; set; }
    [SugarColumn(Length = 32)] public string UsageStatus { get; set; } = string.Empty;
    public bool IsMeasuring { get; set; }
    [SugarColumn(IsNullable = true)] public DateOnly? CalibrationDate { get; set; }
    [SugarColumn(Length = 32)] public string CalibrationCycle { get; set; } = string.Empty;
    [SugarColumn(IsNullable = true)] public DateOnly? ValidUntil { get; set; }
    [SugarColumn(Length = 32)] public string CalibrationStatus { get; set; } = string.Empty;
    [SugarColumn(Length = 64)] public string CertificateNo { get; set; } = string.Empty;
    [SugarColumn(IsNullable = true)] public Guid? CertificateStoredFileId { get; set; }
    [SugarColumn(IsNullable = true)] public DateOnly? LastMaintenance { get; set; }
    [SugarColumn(Length = 512)] public string MaintenanceContent { get; set; } = string.Empty;
    [SugarColumn(Length = 32)] public string MaintenanceCycle { get; set; } = string.Empty;
    [SugarColumn(IsNullable = true)] public DateOnly? NextMaintenance { get; set; }
    [SugarColumn(IsNullable = true)] public Guid? MaintenanceStoredFileId { get; set; }
    [SugarColumn(IsNullable = true)] public int? SuggestedUses { get; set; }
    public int UsedCount { get; set; }
    [SugarColumn(IsNullable = true)] public int? SuggestedYears { get; set; }
    [SugarColumn(IsNullable = true)] public Guid? OwnerPersonId { get; set; }
    [SugarColumn(Length = 64)] public string Department { get; set; } = string.Empty;
    [SugarColumn(Length = 64)] public string Area { get; set; } = string.Empty;
    [SugarColumn(Length = 64)] public string Rig { get; set; } = string.Empty;
    [SugarColumn(Length = 32)] public string RigCode { get; set; } = string.Empty;
    public SystemType101 System { get; set; }
    [SugarColumn(Length = 64)] public string Subsystem { get; set; } = string.Empty;
}

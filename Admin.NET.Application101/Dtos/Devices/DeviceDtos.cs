using Admin.NET.Application101.Dtos.Common;

namespace Admin.NET.Application101.Dtos.Devices;

public sealed class DevicePageQuery : PageQuery
{
    public SystemType101? System { get; set; }
    public string? Status { get; set; }
}

public sealed class DeviceDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FactoryCode { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Range { get; set; } = string.Empty;
    public DateOnly? EnabledDate { get; set; }
    public string UsageStatus { get; set; } = string.Empty;
    public bool IsMeasuring { get; set; }
    public DateOnly? CalibrationDate { get; set; }
    public string CalibrationCycle { get; set; } = string.Empty;
    public DateOnly? ValidUntil { get; set; }
    public string CalibrationStatus { get; set; } = string.Empty;
    public string CertificateNo { get; set; } = string.Empty;
    public Guid? CertificateStoredFileId { get; set; }
    public DateOnly? LastMaintenance { get; set; }
    public string MaintenanceContent { get; set; } = string.Empty;
    public string MaintenanceCycle { get; set; } = string.Empty;
    public DateOnly? NextMaintenance { get; set; }
    public Guid? MaintenanceStoredFileId { get; set; }
    public int? SuggestedUses { get; set; }
    public int UsedCount { get; set; }
    public int? SuggestedYears { get; set; }
    public Guid? OwnerPersonId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Rig { get; set; } = string.Empty;
    public string RigCode { get; set; } = string.Empty;
    public SystemType101 System { get; set; }
    public string Subsystem { get; set; } = string.Empty;
}

public class CreateDeviceInput
{
    [Required, RegularExpression(@".*\S.*")] public string Code { get; set; } = string.Empty;
    [Required, RegularExpression(@".*\S.*")] public string Name { get; set; } = string.Empty;
    public string FactoryCode { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Range { get; set; } = string.Empty;
    public DateOnly? EnabledDate { get; set; }
    public string UsageStatus { get; set; } = string.Empty;
    public bool IsMeasuring { get; set; }
    public DateOnly? CalibrationDate { get; set; }
    public string CalibrationCycle { get; set; } = string.Empty;
    public DateOnly? ValidUntil { get; set; }
    public string CalibrationStatus { get; set; } = string.Empty;
    public string CertificateNo { get; set; } = string.Empty;
    public Guid? CertificateStoredFileId { get; set; }
    public DateOnly? LastMaintenance { get; set; }
    public string MaintenanceContent { get; set; } = string.Empty;
    public string MaintenanceCycle { get; set; } = string.Empty;
    public DateOnly? NextMaintenance { get; set; }
    public Guid? MaintenanceStoredFileId { get; set; }
    [Range(0, int.MaxValue)] public int? SuggestedUses { get; set; }
    [Range(0, int.MaxValue)] public int UsedCount { get; set; }
    [Range(0, int.MaxValue)] public int? SuggestedYears { get; set; }
    public Guid? OwnerPersonId { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Rig { get; set; } = string.Empty;
    public string RigCode { get; set; } = string.Empty;
    public SystemType101 System { get; set; }
    public string Subsystem { get; set; } = string.Empty;
}

public sealed class UpdateDeviceInput : CreateDeviceInput;

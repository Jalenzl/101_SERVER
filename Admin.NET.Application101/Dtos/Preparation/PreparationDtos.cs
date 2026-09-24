using Newtonsoft.Json.Linq;

namespace Admin.NET.Application101.Dtos.Preparation;

public sealed record TaskPlanRowDto(Guid Id, DateOnly? CompletedDate, string Content, string OwnerUnit,
    string SupportUnit, string Remark, int Order);
public sealed record TaskPlanDto(Guid TaskId, DateTime? IgnitionTime, IReadOnlyList<TaskPlanRowDto> Rows);
public sealed record TaskPlanRowInput(DateOnly? CompletedDate, string Content, string OwnerUnit,
    string SupportUnit, string Remark, int Order);
public sealed record SaveTaskPlanInput(DateTime? IgnitionTime, IReadOnlyList<TaskPlanRowInput> Rows);

public sealed record TaskTeamMemberInput(string Role, Guid PersonId, int Order);
public sealed record TaskPostInput(SystemType101 System, string Rig, string PostName, string PostCode,
    Guid PersonId, int Order);
public sealed record SaveTaskPersonnelInput(IReadOnlyList<TaskTeamMemberInput> Team,
    IReadOnlyList<TaskPostInput> Posts);

public sealed record TaskDeviceSelectionInput(SystemType101 System, IReadOnlyList<Guid> DeviceIds);
public sealed record SaveTaskDevicesInput(IReadOnlyList<TaskDeviceSelectionInput> Systems);
public sealed record TaskDocumentSelectionInput(string DocumentType, IReadOnlyList<Guid> DocumentIds);
public sealed record SaveTaskDocumentsInput(IReadOnlyList<TaskDocumentSelectionInput> Types);

public sealed record TaskPersonnelDto(Guid Id, Guid PersonId, string Kind, string Role, SystemType101? System,
    string Rig, string PostName, string PostCode, int Order);
public sealed record TaskDeviceDto(Guid Id, Guid DeviceId, SystemType101 System, int Order);
public sealed record TaskDocumentDto(Guid Id, Guid DocumentId, string DocumentType, int Order);
public sealed record TaskPreparationDto(IReadOnlyList<TaskPersonnelDto> Personnel,
    IReadOnlyList<TaskDeviceDto> Devices, IReadOnlyList<TaskDocumentDto> Documents);

public sealed record TransferRowInput(Guid Id, int Order, JObject Data);
public sealed record SaveTransferInput(IReadOnlyList<TransferRowInput> Rows);
public sealed record TransferRowDto(Guid Id, int Order, JObject Data);

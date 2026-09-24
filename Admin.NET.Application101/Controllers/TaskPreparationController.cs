using Admin.NET.Application101.Dtos.Preparation;
using Admin.NET.Application101.Services;

namespace Admin.NET.Application101.Controllers;

[ApiController]
[Route("api/101/tasks/{taskId:guid}")]
public sealed class TaskPreparationController(TaskPreparationService101 service) : ControllerBase
{
    [HttpGet("plan"), ApiPermission("101:plan:read")]
    public Task<TaskPlanDto> GetPlan(Guid taskId) => service.GetPlanAsync(taskId);

    [HttpPut("plan"), ApiPermission("101:plan:update")]
    public Task SavePlan(Guid taskId, SaveTaskPlanInput input) => service.SavePlanAsync(taskId, input);

    [HttpGet("preparation"), ApiPermission("101:preparation:read")]
    public Task<TaskPreparationDto> GetPreparation(Guid taskId) => service.GetPreparationAsync(taskId);

    [HttpPut("personnel"), ApiPermission("101:preparation:personnel")]
    public Task SavePersonnel(Guid taskId, SaveTaskPersonnelInput input) => service.SavePersonnelAsync(taskId, input);

    [HttpPut("devices"), ApiPermission("101:preparation:device")]
    public Task SaveDevices(Guid taskId, SaveTaskDevicesInput input) => service.SaveDevicesAsync(taskId, input);

    [HttpPut("documents"), ApiPermission("101:preparation:document")]
    public Task SaveDocuments(Guid taskId, SaveTaskDocumentsInput input) => service.SaveDocumentsAsync(taskId, input);

    [HttpGet("transfers/{tableId}"), ApiPermission("101:preparation:transfer:read")]
    public Task<IReadOnlyList<TransferRowDto>> GetTransfers(Guid taskId, string tableId) =>
        service.GetTransfersAsync(taskId, tableId);

    [HttpPut("transfers/{tableId}"), RequestSizeLimit(16 * 1024 * 1024), ApiPermission("101:preparation:transfer:update")]
    public Task SaveTransfers(Guid taskId, string tableId, SaveTransferInput input) =>
        service.SaveTransfersAsync(taskId, tableId, input);
}

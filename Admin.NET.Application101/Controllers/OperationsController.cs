using Admin.NET.Application101.Dtos.Operations;
using Admin.NET.Application101.Services;

namespace Admin.NET.Application101.Controllers;

[ApiController]
[Route("api/101")]
public sealed class OperationsController(OperationService101 service) : ControllerBase
{
    [HttpGet("tasks/{taskId:guid}/operations"), ApiPermission("101:operation:read")]
    public Task<IReadOnlyList<OperationDto>> List(Guid taskId) => service.ListAsync(taskId);

    [HttpGet("operations/{id:guid}"), ApiPermission("101:operation:read")]
    public Task<OperationDto> Get(Guid id) => service.GetAsync(id);

    [HttpPut("operations/{id:guid}"), ApiPermission("101:operation:update")]
    public Task Update(Guid id, UpdateOperationInput input) => service.UpdateAsync(id, input);

    [HttpPut("operations/{id:guid}/checks"), ApiPermission("101:operation:check")]
    public Task SaveChecks(Guid id, SaveOperationChecksInput input) => service.SaveChecksAsync(id, input);

    [HttpPost("operations/{id:guid}/signatures"), ApiPermission("101:operation:sign")]
    public Task<OperationSignatureDto> Sign(Guid id, CreateSignatureInput input) => service.SignAsync(id, input);

    [HttpDelete("operations/{id:guid}/signatures/{role}"), ApiPermission("101:operation:withdraw-signature")]
    public Task WithdrawSignature(Guid id, string role) => service.WithdrawSignatureAsync(id, role);
}

using Admin.NET.Application101.Dtos.Records;
using Admin.NET.Application101.Services;

namespace Admin.NET.Application101.Controllers;

[ApiController]
[Route("api/101/tasks/{taskId:guid}/signatures/{section}/{qualifier}")]
public sealed class PreparationSignaturesController(PreparationSignatureService101 service) : ControllerBase
{
    [HttpGet, ApiPermission("101:preparation:signature:read")]
    public Task<IReadOnlyList<SignatureDto>> List(Guid taskId, string section, string qualifier) =>
        service.ListAsync(taskId, section, qualifier);

    [HttpPost, ApiPermission("101:preparation:signature:sign")]
    public Task<SignatureDto> Sign(Guid taskId, string section, string qualifier, SignRecordInput input) =>
        service.SignAsync(taskId, section, qualifier, input);

    [HttpDelete("{role}"), ApiPermission("101:preparation:signature:withdraw")]
    public Task Withdraw(Guid taskId, string section, string qualifier, string role) =>
        service.WithdrawAsync(taskId, section, qualifier, role);
}

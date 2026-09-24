using Admin.NET.Application101.Dtos.Common;
using Admin.NET.Application101.Dtos.Devices;
using Admin.NET.Application101.Services;
using Microsoft.AspNetCore.Http;

namespace Admin.NET.Application101.Controllers;

[ApiController]
[Route("api/101/devices")]
public sealed class DevicesController(DeviceService101 service, DeviceFileService101 files) : ControllerBase
{
    [HttpGet, ApiPermission("101:device:read")]
    public Task<PageResult<DeviceDto>> Page([FromQuery] DevicePageQuery input) => service.PageAsync(input);

    [HttpPost, ApiPermission("101:device:create")]
    public Task<Guid> Create(CreateDeviceInput input) => service.CreateAsync(input);

    [HttpGet("{id:guid}"), ApiPermission("101:device:read")]
    public Task<DeviceDto> Get(Guid id) => service.GetAsync(id);

    [HttpPut("{id:guid}"), ApiPermission("101:device:update")]
    public Task Update(Guid id, UpdateDeviceInput input) => service.UpdateAsync(id, input);

    [HttpDelete("{id:guid}"), ApiPermission("101:device:delete")]
    public Task Delete(Guid id) => service.DeleteAsync(id);

    [HttpPost("{id:guid}/files/{kind}"), ApiPermission("101:file:upload")]
    public async Task<Guid> Upload(Guid id, string kind, [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        return await files.UploadAsync(id, kind, file.FileName, file.ContentType, stream, cancellationToken);
    }
}

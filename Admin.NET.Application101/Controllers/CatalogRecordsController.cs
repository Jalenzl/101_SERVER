using Admin.NET.Application101.Dtos.Common;
using Admin.NET.Application101.Services;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace Admin.NET.Application101.Controllers;

[ApiController]
[Route("api/101/catalogs/{key}")]
public sealed class CatalogRecordsController(CatalogRecordService101 service, CatalogFileService101 files) : ControllerBase
{
    [HttpGet, ApiPermission("101:catalog:read")]
    public Task<PageResult<CatalogRecordDto>> Page(string key, [FromQuery] PageQuery query) =>
        service.PageAsync(key, query);

    [HttpGet("{id:guid}"), ApiPermission("101:catalog:read")]
    public Task<CatalogRecordDto> Get(string key, Guid id) => service.GetAsync(key, id);

    [HttpPost, RequestSizeLimit(1024 * 1024), ApiPermission("101:catalog:write")]
    public Task<Guid> Create(string key, [FromBody] JObject data) => service.CreateAsync(key, data);

    [HttpPut("{id:guid}"), RequestSizeLimit(1024 * 1024), ApiPermission("101:catalog:write")]
    public Task Update(string key, Guid id, [FromBody] JObject data) => service.UpdateAsync(key, id, data);

    [HttpDelete("{id:guid}"), ApiPermission("101:catalog:write")]
    public Task Delete(string key, Guid id) => service.DeleteAsync(key, id);

    [HttpPost("{id:guid}/files"), ApiPermission("101:file:upload")]
    public async Task<Guid> Upload(string key, Guid id, [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        return await files.UploadAsync(key, id, file.FileName, file.ContentType, stream, cancellationToken);
    }
}

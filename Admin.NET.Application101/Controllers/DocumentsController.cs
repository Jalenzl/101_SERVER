using Admin.NET.Application101.Dtos.Common;
using Admin.NET.Application101.Dtos.Documents;
using Admin.NET.Application101.Services;
using Microsoft.AspNetCore.Http;

namespace Admin.NET.Application101.Controllers;

[ApiController]
[Route("api/101/documents")]
public sealed class DocumentsController(DocumentService101 service) : ControllerBase
{
    [HttpGet, ApiPermission("101:document:read")]
    public Task<PageResult<DocumentDto>> Page([FromQuery] DocumentPageQuery input) => service.PageAsync(input);

    [HttpPost, ApiPermission("101:document:create")]
    public Task<Guid> Create(CreateDocumentInput input) => service.CreateAsync(input);

    [HttpGet("{id:guid}"), ApiPermission("101:document:read")]
    public Task<DocumentDto> Get(Guid id) => service.GetAsync(id);

    [HttpPut("{id:guid}"), ApiPermission("101:document:update")]
    public Task Update(Guid id, UpdateDocumentInput input) => service.UpdateAsync(id, input);

    [HttpDelete("{id:guid}"), ApiPermission("101:document:delete")]
    public Task Delete(Guid id) => service.DeleteAsync(id);

    [HttpPost("{id:guid}/files"), ApiPermission("101:file:upload")]
    public async Task<Guid> Upload(Guid id, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        return await service.UploadAsync(id, file.FileName, file.ContentType, stream, cancellationToken);
    }
}

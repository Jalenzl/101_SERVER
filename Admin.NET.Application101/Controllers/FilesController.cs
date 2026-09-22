using Admin.NET.Application101.Services;

namespace Admin.NET.Application101.Controllers;

[ApiController]
[Route("api/101/files")]
public sealed class FilesController(DocumentService101 service) : ControllerBase
{
    [HttpGet("{id:guid}/download"), ApiPermission("101:file:download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var file = await service.DownloadAsync(id, cancellationToken);
        return File(file.Stream, file.ContentType, file.OriginalName);
    }

    [HttpDelete("{id:guid}"), ApiPermission("101:file:delete")]
    public Task Delete(Guid id, CancellationToken cancellationToken) => service.DeleteFileAsync(id, cancellationToken);
}

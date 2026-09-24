using Admin.NET.Application101.Dtos.Records;
using Admin.NET.Application101.Services;

namespace Admin.NET.Application101.Controllers;

[ApiController]
[Route("api/101/tasks/{taskId:guid}/records/{kind}")]
public sealed class TaskRecordsController(TaskRecordService101 service) : ControllerBase
{
    [HttpGet, ApiPermission("101:task-record:read")]
    public Task<IReadOnlyList<RecordRowDto>> Get(Guid taskId, string kind) => service.GetAsync(taskId, kind);

    [HttpPut, RequestSizeLimit(16 * 1024 * 1024), ApiPermission("101:task-record:update")]
    public Task Save(Guid taskId, string kind, SaveRecordRowsInput input) => service.SaveAsync(taskId, kind, input);
}

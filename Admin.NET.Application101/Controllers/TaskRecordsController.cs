using Admin.NET.Application101.Dtos.Records;
using Admin.NET.Application101.Services;

namespace Admin.NET.Application101.Controllers;

[ApiController]
[Route("api/101/tasks/{taskId:guid}/records")]
public sealed class TaskRecordsController(TaskRecordService101 service) : ControllerBase
{
    [HttpGet("fmeca"), ApiPermission("101:task-record:fmeca:read")]
    public Task<IReadOnlyList<RecordRowDto>> GetFmeca(Guid taskId) => service.GetAsync(taskId, "fmeca");

    [HttpPut("fmeca"), RequestSizeLimit(16 * 1024 * 1024), ApiPermission("101:task-record:fmeca:update")]
    public Task SaveFmeca(Guid taskId, SaveRecordRowsInput input) => service.SaveAsync(taskId, "fmeca", input);

    [HttpGet("fmea"), ApiPermission("101:task-record:fmea:read")]
    public Task<IReadOnlyList<RecordRowDto>> GetFmea(Guid taskId) => service.GetAsync(taskId, "fmea");

    [HttpPut("fmea"), RequestSizeLimit(16 * 1024 * 1024), ApiPermission("101:task-record:fmea:update")]
    public Task SaveFmea(Guid taskId, SaveRecordRowsInput input) => service.SaveAsync(taskId, "fmea", input);

    [HttpGet("task-risk"), ApiPermission("101:task-record:task-risk:read")]
    public Task<IReadOnlyList<RecordRowDto>> GetTaskRisk(Guid taskId) => service.GetAsync(taskId, "task-risk");

    [HttpPut("task-risk"), RequestSizeLimit(16 * 1024 * 1024), ApiPermission("101:task-record:task-risk:update")]
    public Task SaveTaskRisk(Guid taskId, SaveRecordRowsInput input) => service.SaveAsync(taskId, "task-risk", input);

    [HttpGet("summary-{part}"), ApiPermission("101:task-record:summary:read")]
    public Task<IReadOnlyList<RecordRowDto>> GetSummary(Guid taskId, string part) =>
        service.GetAsync(taskId, $"summary-{part}");

    [HttpPut("summary-{part}"), RequestSizeLimit(16 * 1024 * 1024), ApiPermission("101:task-record:summary:update")]
    public Task SaveSummary(Guid taskId, string part, SaveRecordRowsInput input) =>
        service.SaveAsync(taskId, $"summary-{part}", input);

    [HttpGet("stops"), ApiPermission("101:task-record:stops:read")]
    public Task<IReadOnlyList<RecordRowDto>> GetStops(Guid taskId) => service.GetAsync(taskId, "stops");

    [HttpPut("stops"), RequestSizeLimit(16 * 1024 * 1024), ApiPermission("101:task-record:stops:update")]
    public Task SaveStops(Guid taskId, SaveRecordRowsInput input) => service.SaveAsync(taskId, "stops", input);
}

using Admin.NET.Application101.Dtos.Common;
using Admin.NET.Application101.Dtos.Tasks;
using Admin.NET.Application101.Dtos.Operations;
using Admin.NET.Application101.Services;

namespace Admin.NET.Application101.Controllers;

[ApiController]
[Route("api/101/tasks")]
public sealed class TasksController(TaskService101 service) : ControllerBase
{
    [HttpGet]
    [ApiPermission("101:task:read")]
    public Task<PageResult<TaskDto>> Page([FromQuery] TaskPageQuery input) => service.PageAsync(input);

    [HttpPost]
    [ApiPermission("101:task:create")]
    public Task<Guid> Create(CreateTaskInput input) => service.CreateAsync(input);

    [HttpGet("{id:guid}")]
    [ApiPermission("101:task:read")]
    public Task<TaskDto> Get(Guid id) => service.GetAsync(id);

    [HttpPut("{id:guid}")]
    [ApiPermission("101:task:update")]
    public Task Update(Guid id, UpdateTaskInput input) => service.UpdateAsync(id, input);

    [HttpDelete("{id:guid}")]
    [ApiPermission("101:task:delete")]
    public Task Delete(Guid id) => service.DeleteAsync(id);

    [HttpPut("{id:guid}/status")]
    [ApiPermission("101:task:status")]
    public Task SetStatus(Guid id, ChangeTaskStatusInput input) => service.SetStatusAsync(id, input.Status);

    [HttpGet("{id:guid}/workflow")]
    [ApiPermission("101:workflow-selection:read")]
    public Task<TaskWorkflowSelectionDto> GetWorkflow(Guid id) => service.GetWorkflowAsync(id);

    [HttpPut("{id:guid}/workflow")]
    [ApiPermission("101:workflow-selection:update")]
    public Task SaveWorkflow(Guid id, SaveTaskWorkflowInput input) => service.SaveWorkflowAsync(id, input);
}

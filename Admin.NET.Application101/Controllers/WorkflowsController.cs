using Admin.NET.Application101.Dtos.Common;
using Admin.NET.Application101.Dtos.Workflows;
using Admin.NET.Application101.Services;

namespace Admin.NET.Application101.Controllers;

[ApiController]
[Route("api/101/workflows")]
public sealed class WorkflowsController(WorkflowService101 service) : ControllerBase
{
    [HttpGet, ApiPermission("101:workflow:read")]
    public Task<PageResult<WorkflowDto>> Page([FromQuery] WorkflowPageQuery input) => service.PageAsync(input);

    [HttpGet("tree"), ApiPermission("101:workflow:read")]
    public Task<IReadOnlyList<WorkflowTreeNodeDto>> Tree([FromQuery] string? department) => service.TreeAsync(department);

    [HttpPost, ApiPermission("101:workflow:create")]
    public Task<Guid> Create(CreateWorkflowInput input) => service.CreateAsync(input);

    [HttpPut("{id:guid}"), ApiPermission("101:workflow:update")]
    public Task Update(Guid id, UpdateWorkflowInput input) => service.UpdateAsync(id, input);

    [HttpDelete("{id:guid}"), ApiPermission("101:workflow:delete")]
    public Task Delete(Guid id) => service.DeleteAsync(id);
}

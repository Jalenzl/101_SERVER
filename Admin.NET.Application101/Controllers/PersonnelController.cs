using Admin.NET.Application101.Dtos.Common;
using Admin.NET.Application101.Dtos.Personnel;
using Admin.NET.Application101.Services;

namespace Admin.NET.Application101.Controllers;

[ApiController]
[Route("api/101/personnel")]
public sealed class PersonnelController(PersonnelService101 service) : ControllerBase
{
    [HttpGet, ApiPermission("101:person:read")]
    public Task<PageResult<PersonDto>> Page([FromQuery] PersonPageQuery input) => service.PageAsync(input);

    [HttpPost, ApiPermission("101:person:create")]
    public Task<Guid> Create(CreatePersonInput input) => service.CreateAsync(input);

    [HttpGet("{id:guid}"), ApiPermission("101:person:read")]
    public Task<PersonDto> Get(Guid id) => service.GetAsync(id);

    [HttpPut("{id:guid}"), ApiPermission("101:person:update")]
    public Task Update(Guid id, UpdatePersonInput input) => service.UpdateAsync(id, input);

    [HttpDelete("{id:guid}"), ApiPermission("101:person:delete")]
    public Task Delete(Guid id) => service.DeleteAsync(id);
}

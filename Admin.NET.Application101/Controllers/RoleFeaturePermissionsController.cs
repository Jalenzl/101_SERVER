using Admin.NET.Application101.Services;
using Microsoft.AspNetCore.Mvc;

namespace Admin.NET.Application101.Controllers;

[ApiController]
[Route("api/101/role-features")]
public sealed class RoleFeaturePermissionsController(RoleFeaturePermissionService101 service) : ControllerBase
{
    [HttpGet("mine"), ApiPermission("101:catalog:read")]
    public Task<MyFeaturePermissions101> Mine() => service.GetMineAsync();

    [HttpGet("{roleId:long}"), ApiPermission("sysRole:page")]
    public Task<RoleFeatureRule101[]> Get(long roleId) => service.GetRoleAsync(roleId);

    [HttpPut("{roleId:long}"), ApiPermission("sysRole:grantMenu")]
    public Task Save(long roleId, [FromBody] RoleFeatureRule101[] rules) => service.SaveRoleAsync(roleId, rules);
}

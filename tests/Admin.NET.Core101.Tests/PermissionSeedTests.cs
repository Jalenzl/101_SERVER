using System.Reflection;
using Admin.NET.Application101.Authorization;
using Admin.NET.Application101.Controllers;
using Admin.NET.Core;
using Admin.NET.Core101.Seed;
using Microsoft.AspNetCore.Mvc;

namespace Admin.NET.Core101.Tests;

public sealed class PermissionSeedTests
{
    private static readonly string[] Expected =
    {
        "101:task:read", "101:task:create", "101:task:update", "101:task:delete", "101:task:status",
        "101:person:read", "101:person:create", "101:person:update", "101:person:delete",
        "101:device:read", "101:device:create", "101:device:update", "101:device:delete",
        "101:document:read", "101:document:create", "101:document:update", "101:document:delete",
        "101:file:upload", "101:file:download", "101:file:delete",
        "101:workflow:read", "101:workflow:create", "101:workflow:update", "101:workflow:delete",
        "101:plan:read", "101:plan:update", "101:preparation:read", "101:preparation:personnel",
        "101:preparation:device", "101:preparation:document", "101:preparation:transfer:read",
        "101:preparation:transfer:update", "101:workflow-selection:read", "101:workflow-selection:update",
        "101:operation:read", "101:operation:update", "101:operation:check", "101:operation:sign",
        "101:operation:withdraw-signature"
    };

    [Fact]
    public void ControllerPermissions_ExactlyMatchSeededButtons()
    {
        var controllerPermissions = typeof(TasksController).Assembly.GetTypes()
            .Where(type => typeof(ControllerBase).IsAssignableFrom(type))
            .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            .Select(method => method.GetCustomAttribute<ApiPermissionAttribute>()?.Name)
            .Where(name => name is not null).Cast<string>().ToHashSet();
        var seededPermissions = MenuSeed101.Menus.Where(item => item.Type == MenuTypeEnum.Btn)
            .Select(item => item.Permission!).ToHashSet();

        Assert.Equal(Expected.ToHashSet(), controllerPermissions);
        Assert.Equal(Expected.ToHashSet(), seededPermissions);
    }

    [Fact]
    public void MenuAndRoleMenuIds_AreStableUniqueAndOutsideAdminSeedRange()
    {
        var menuIds = MenuSeed101.Menus.Select(item => item.Id).ToArray();
        var roleMenuIds = RoleMenuSeed101.Items.Select(item => item.Id).ToArray();
        Assert.Equal(menuIds.Length, menuIds.Distinct().Count());
        Assert.Equal(roleMenuIds.Length, roleMenuIds.Distinct().Count());
        Assert.All(menuIds, id => Assert.True(id >= 1501010000001));
        Assert.All(roleMenuIds, id => Assert.True(id >= 1501011000001));
        Assert.All(RoleMenuSeed101.Items, item => Assert.Equal(1300000000101, item.RoleId));
    }
}

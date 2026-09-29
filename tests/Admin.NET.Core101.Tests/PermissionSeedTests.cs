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
        "101:operation:withdraw-signature", "101:task-record:read", "101:task-record:update",
        "101:catalog:read", "101:catalog:write", "101:preparation:signature:read",
        "101:preparation:signature:sign", "101:preparation:signature:withdraw"
    };

    private static readonly string[] NewTaskRecordPermissions =
    {
        "101:task-record:fmeca:read", "101:task-record:fmeca:update",
        "101:task-record:fmea:read", "101:task-record:fmea:update",
        "101:task-record:task-risk:read", "101:task-record:task-risk:update",
        "101:task-record:summary:read", "101:task-record:summary:update",
        "101:task-record:stops:read", "101:task-record:stops:update"
    };

    [Fact]
    public void ControllerPermissions_AreSeededWhileNewTaskRecordButtonsAreAppended()
    {
        var controllerPermissions = typeof(TasksController).Assembly.GetTypes()
            .Where(type => typeof(ControllerBase).IsAssignableFrom(type))
            .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            .Select(method => method.GetCustomAttribute<ApiPermissionAttribute>()?.Name)
            .Where(name => name is not null && name.StartsWith("101:", StringComparison.Ordinal))
            .Cast<string>().ToHashSet();
        var seededPermissions = MenuSeed101.Menus.Where(item => item.Type == MenuTypeEnum.Btn)
            .Select(item => item.Permission!).ToHashSet();

        Assert.True(controllerPermissions.IsSubsetOf(seededPermissions),
            $"Unseeded: {string.Join(", ", controllerPermissions.Except(seededPermissions))}");
        Assert.Contains("101:task-record:read", controllerPermissions);
        Assert.Contains("101:task-record:update", controllerPermissions);
        Assert.True(Expected.Concat(NewTaskRecordPermissions).ToHashSet().SetEquals(seededPermissions));
    }

    [Fact]
    public void TaskRecordButtons_KeepExistingIdsAndGrantOnlyNewButtonsToAdministrator()
    {
        var buttons = MenuSeed101.Menus.Where(item => item.Type == MenuTypeEnum.Btn).ToArray();
        Assert.Equal(Expected.Length + NewTaskRecordPermissions.Length, buttons.Length);
        Assert.All(Expected.Select((permission, index) => (permission, index)), entry =>
        {
            Assert.Equal(entry.permission, buttons[entry.index].Permission);
            Assert.Equal(1501010000101 + entry.index, buttons[entry.index].Id);
        });
        Assert.All(NewTaskRecordPermissions.Select((permission, index) => (permission, index)), entry =>
        {
            var button = buttons[Expected.Length + entry.index];
            Assert.Equal(entry.permission, button.Permission);
            Assert.Equal(1501010000101 + Expected.Length + entry.index, button.Id);
        });

        var grants = RoleMenuSeed101.Items.Select(item => item.MenuId).ToHashSet();
        Assert.All(NewTaskRecordPermissions, permission =>
            Assert.Contains(buttons.Single(item => item.Permission == permission).Id, grants));
        Assert.DoesNotContain(buttons.Single(item => item.Permission == "101:task-record:read").Id, grants);
        Assert.DoesNotContain(buttons.Single(item => item.Permission == "101:task-record:update").Id, grants);
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

    [Fact]
    public void GrantUserRole_IsASeededButtonGrantedToSystemAdministrator()
    {
        var button = Assert.Single(new SysMenuSeedData().HasData(),
            item => item.Permission == "sysUser:grantRole");
        Assert.Equal(MenuTypeEnum.Btn, button.Type);
        Assert.Contains(new SysRoleMenuSeedData().HasData(),
            item => item.MenuId == button.Id && item.RoleId == RoleMenuSeed101.SystemAdministratorRoleId);
    }

    [Fact]
    public void SystemAdministrator_CanManageRoles()
    {
        var assigned = RoleMenuSeed101.Items.Select(item => item.MenuId).ToHashSet();
        Assert.All(Enumerable.Range(0, 6), index =>
            Assert.Contains(1310000000121 + index, assigned));
    }

}

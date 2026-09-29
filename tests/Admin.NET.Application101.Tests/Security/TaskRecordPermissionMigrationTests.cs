using Admin.NET.Application101.Services;
using Admin.NET.Core;
using Admin.NET.Core101.Seed;
using Xunit;

namespace Admin.NET.Application101.Tests.Security;

public sealed class TaskRecordPermissionMigrationTests
{
    private static long Id(string permission) => MenuSeed101.Menus.Single(menu => menu.Permission == permission).Id;

    [Fact]
    public void OldReadAndEdit_AreCopiedOnlyToMatchingNewOperations()
    {
        var relations = new[]
        {
            new SysRoleMenu { RoleId = 11, MenuId = Id("101:task-record:read") },
            new SysRoleMenu { RoleId = 22, MenuId = Id("101:task-record:update") }
        };

        var grants = TaskRecordPermissionMigration101.MissingGrants(relations);

        Assert.Equal(10, grants.Count);
        Assert.All(grants.Where(grant => grant.RoleId == 11), grant =>
            Assert.EndsWith(":read", MenuSeed101.Menus.Single(menu => menu.Id == grant.MenuId).Permission));
        Assert.All(grants.Where(grant => grant.RoleId == 22), grant =>
            Assert.EndsWith(":update", MenuSeed101.Menus.Single(menu => menu.Id == grant.MenuId).Permission));
        Assert.Equal(5, grants.Count(grant => grant.RoleId == 11));
        Assert.Equal(5, grants.Count(grant => grant.RoleId == 22));
    }

    [Fact]
    public void ExistingNewGrant_IsNotDuplicated()
    {
        var relations = new[]
        {
            new SysRoleMenu { RoleId = 11, MenuId = Id("101:task-record:read") },
            new SysRoleMenu { RoleId = 11, MenuId = Id("101:task-record:fmeca:read") }
        };

        var grants = TaskRecordPermissionMigration101.MissingGrants(relations);

        Assert.Equal(4, grants.Count);
        Assert.DoesNotContain(grants, grant => grant.MenuId == Id("101:task-record:fmeca:read"));
    }

    [Fact]
    public void AfterOldRelationsRemoved_RevokedPageIsNotRestoredOnRestart()
    {
        var remaining = new[]
        {
            new SysRoleMenu { RoleId = 11, MenuId = Id("101:task-record:fmea:read") }
        };

        Assert.Empty(TaskRecordPermissionMigration101.MissingGrants(remaining));
    }
}

using System.Reflection;
using Admin.NET.Application101;
using Admin.NET.Core;
using Admin.NET.Core101.Seed;
using SqlSugar;
using Xunit;

namespace Admin.NET.Application101.Tests.Security;

public sealed class TaskRecordPermissionStartupTests
{
    [Fact]
    public async Task FailedFirstInitialization_RollsBackRootSoRetryCanSeedAdministrator()
    {
        var path = Path.Combine(Path.GetTempPath(), $"tcp101-permissions-{Guid.NewGuid():N}.db");
        try
        {
            using var database = new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = $"DataSource={path};Pooling=False", DbType = DbType.Sqlite,
                IsAutoCloseConnection = true, InitKeyType = InitKeyType.Attribute
            });
            database.CodeFirst.InitTables(typeof(SysMenu), typeof(SysRoleMenu));
            // SQLite CodeFirst marks optional menu columns required; the production seed leaves them null.
            await database.Ado.ExecuteCommandAsync("""
                CREATE TABLE SysMenuRelaxed AS SELECT * FROM SysMenu WHERE 0;
                DROP TABLE SysMenu;
                ALTER TABLE SysMenuRelaxed RENAME TO SysMenu;
                """);
            await database.Ado.ExecuteCommandAsync($"""
                CREATE TRIGGER fail_admin_grant BEFORE INSERT ON "SysRoleMenu"
                WHEN NEW."MenuId" = {MenuSeed101.RootId}
                BEGIN SELECT RAISE(ABORT, 'admin grant failed'); END;
                """);

            await Assert.ThrowsAnyAsync<Exception>(() => SeedPermissionsAsync(database));
            Assert.False(await database.Queryable<SysMenu>().AnyAsync(item => item.Id == MenuSeed101.RootId));

            await database.Ado.ExecuteCommandAsync("DROP TRIGGER fail_admin_grant");
            await SeedPermissionsAsync(database);
            Assert.True(await database.Queryable<SysMenu>().AnyAsync(item => item.Id == MenuSeed101.RootId));
            var grants = await database.Queryable<SysRoleMenu>()
                .Where(item => item.RoleId == RoleMenuSeed101.SystemAdministratorRoleId).ToListAsync();
            var taskRecordIds = MenuSeed101.Menus.Where(item => item.Permission?.StartsWith("101:task-record:") == true &&
                item.Permission is not "101:task-record:read" and not "101:task-record:update")
                .Select(item => item.Id).ToHashSet();
            Assert.Equal(10, grants.Count(item => taskRecordIds.Contains(item.MenuId)));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    private static Task SeedPermissionsAsync(ISqlSugarClient database) =>
        (Task)typeof(Startup).GetMethod("SeedPermissionsAsync", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, new object[] { database })!;
}

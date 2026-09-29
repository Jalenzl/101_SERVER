namespace Admin.NET.Core101.Seed;

public static class RoleMenuSeed101
{
    public const long SystemAdministratorRoleId = 1300000000101;

    private static readonly HashSet<long> LegacyTaskRecordMenuIds = MenuSeed101.Menus
        .Where(menu => menu.Permission is "101:task-record:read" or "101:task-record:update")
        .Select(menu => menu.Id).ToHashSet();

    public static IReadOnlyList<SysRoleMenu> Items { get; } = MenuSeed101.Menus
        .Select((menu, index) => new SysRoleMenu
        {
            Id = 1501011000001 + index,
            RoleId = SystemAdministratorRoleId,
            MenuId = menu.Id
        })
        .Where(item => !LegacyTaskRecordMenuIds.Contains(item.MenuId))
        .Concat(Enumerable.Range(0, 6).Select(index => new SysRoleMenu
        {
            Id = 1501011000901 + index,
            RoleId = SystemAdministratorRoleId,
            MenuId = 1310000000121 + index
        }))
        .ToArray();
}

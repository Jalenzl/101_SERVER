namespace Admin.NET.Core101.Seed;

public static class RoleMenuSeed101
{
    public const long SystemAdministratorRoleId = 1300000000101;

    public static IReadOnlyList<SysRoleMenu> Items { get; } = MenuSeed101.Menus
        .Select((menu, index) => new SysRoleMenu
        {
            Id = 1501011000001 + index,
            RoleId = SystemAdministratorRoleId,
            MenuId = menu.Id
        })
        .Concat(Enumerable.Range(0, 6).Select(index => new SysRoleMenu
        {
            Id = 1501011000901 + index,
            RoleId = SystemAdministratorRoleId,
            MenuId = 1310000000121 + index
        }))
        .ToArray();
}

using Admin.NET.Core101.Seed;

namespace Admin.NET.Application101.Services;

public static class TaskRecordPermissionMigration101
{
    private static readonly long OldReadId = MenuId("101:task-record:read");
    private static readonly long OldUpdateId = MenuId("101:task-record:update");
    private static readonly long[] NewReadIds = new[] { "fmeca", "fmea", "task-risk", "summary", "stops" }
        .Select(page => MenuId($"101:task-record:{page}:read")).ToArray();
    private static readonly long[] NewUpdateIds = new[] { "fmeca", "fmea", "task-risk", "summary", "stops" }
        .Select(page => MenuId($"101:task-record:{page}:update")).ToArray();

    public static IReadOnlySet<long> LegacyMenuIds { get; } = new HashSet<long> { OldReadId, OldUpdateId };

    public static IReadOnlyList<(long RoleId, long MenuId)> MissingGrants(
        IReadOnlyCollection<SysRoleMenu> relations)
    {
        var existing = relations.Select(item => (item.RoleId, item.MenuId)).ToHashSet();
        var grants = new List<(long RoleId, long MenuId)>();
        foreach (var relation in relations)
        {
            var targets = relation.MenuId == OldReadId ? NewReadIds :
                relation.MenuId == OldUpdateId ? NewUpdateIds : Array.Empty<long>();
            foreach (var menuId in targets)
            {
                if (existing.Add((relation.RoleId, menuId)))
                    grants.Add((relation.RoleId, menuId));
            }
        }
        return grants;
    }

    private static long MenuId(string permission) =>
        MenuSeed101.Menus.Single(item => item.Permission == permission).Id;
}

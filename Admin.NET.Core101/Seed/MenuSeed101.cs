namespace Admin.NET.Core101.Seed;

public static class MenuSeed101
{
    public const long RootId = 1501010000001;
    private static readonly DateTime SeedTime = new(2026, 9, 22, 0, 0, 0, DateTimeKind.Utc);

    public static IReadOnlyList<string> Permissions { get; } = new[]
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

    public static IReadOnlyList<SysMenu> Menus { get; } = Build();

    private static IReadOnlyList<SysMenu> Build()
    {
        var menus = new List<SysMenu>
        {
            Menu(RootId, 0, "101业务", "/101", "tcp101", "Layout", MenuTypeEnum.Dir, 100),
            Menu(1501010000011, RootId, "任务管理", "/101/tasks", "tcp101Tasks", "/101/tasks/index", MenuTypeEnum.Menu, 110),
            Menu(1501010000021, RootId, "人员管理", "/101/personnel", "tcp101Personnel", "/101/personnel/index", MenuTypeEnum.Menu, 120),
            Menu(1501010000031, RootId, "设备管理", "/101/devices", "tcp101Devices", "/101/devices/index", MenuTypeEnum.Menu, 130),
            Menu(1501010000041, RootId, "文件管理", "/101/documents", "tcp101Documents", "/101/documents/index", MenuTypeEnum.Menu, 140),
            Menu(1501010000051, RootId, "流程字典", "/101/workflows", "tcp101Workflows", "/101/workflows/index", MenuTypeEnum.Menu, 150),
            Menu(1501010000061, RootId, "任务准备", "/101/preparation", "tcp101Preparation", "/101/preparation/index", MenuTypeEnum.Menu, 160),
            Menu(1501010000071, RootId, "操作执行", "/101/operations", "tcp101Operations", "/101/operations/index", MenuTypeEnum.Menu, 170),
        };

        for (var index = 0; index < Permissions.Count; index++)
        {
            var permission = Permissions[index];
            menus.Add(new SysMenu
            {
                Id = 1501010000101 + index, Pid = ParentId(permission), Title = permission,
                Permission = permission, Type = MenuTypeEnum.Btn, CreateTime = SeedTime, OrderNo = 100 + index
            });
        }
        return menus;
    }

    private static SysMenu Menu(long id, long pid, string title, string path, string name, string component,
        MenuTypeEnum type, int order) => new()
    {
        Id = id, Pid = pid, Title = title, Path = path, Name = name, Component = component,
        Type = type, CreateTime = SeedTime, OrderNo = order
    };

    private static long ParentId(string permission)
    {
        if (permission.StartsWith("101:workflow-selection:", StringComparison.Ordinal) ||
            permission.StartsWith("101:plan:", StringComparison.Ordinal) ||
            permission.StartsWith("101:preparation:", StringComparison.Ordinal)) return 1501010000061;
        if (permission.StartsWith("101:operation:", StringComparison.Ordinal)) return 1501010000071;
        if (permission.StartsWith("101:person:", StringComparison.Ordinal)) return 1501010000021;
        if (permission.StartsWith("101:device:", StringComparison.Ordinal)) return 1501010000031;
        if (permission.StartsWith("101:document:", StringComparison.Ordinal) ||
            permission.StartsWith("101:file:", StringComparison.Ordinal)) return 1501010000041;
        if (permission.StartsWith("101:workflow:", StringComparison.Ordinal)) return 1501010000051;
        return 1501010000011;
    }
}

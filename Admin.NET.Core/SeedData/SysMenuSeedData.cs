namespace Admin.NET.Core;

/// <summary>
/// 精简后的系统菜单种子。101 领域菜单在 Application101 中追加。
/// </summary>
public class SysMenuSeedData : ISqlSugarEntitySeedData<SysMenu>
{
    private static readonly DateTime SeedTime = DateTime.Parse("2022-02-10 00:00:00");

    public IEnumerable<SysMenu> HasData()
    {
        return new[]
        {
            new SysMenu { Id = 1300000000101, Pid = 0, Title = "工作台", Path = "/dashboard", Name = "dashboard", Component = "Layout", Icon = "ele-HomeFilled", Type = MenuTypeEnum.Dir, CreateTime = SeedTime, OrderNo = 0 },
            new SysMenu { Id = 1300000000111, Pid = 1300000000101, Title = "工作台", Path = "/dashboard/home", Name = "home", Component = "/home/index", IsAffix = true, Icon = "ele-HomeFilled", Type = MenuTypeEnum.Menu, CreateTime = SeedTime, OrderNo = 100 },

            new SysMenu { Id = 1310000000101, Pid = 0, Title = "系统管理", Path = "/system", Name = "system", Component = "Layout", Icon = "ele-Setting", Type = MenuTypeEnum.Dir, CreateTime = SeedTime, OrderNo = 10000 },
            new SysMenu { Id = 1310000000111, Pid = 1310000000101, Title = "账号管理", Path = "/system/user", Name = "sysUser", Component = "/system/user/index", Icon = "ele-User", Type = MenuTypeEnum.Menu, CreateTime = SeedTime, OrderNo = 100 },
            new SysMenu { Id = 1310000000112, Pid = 1310000000111, Title = "查询", Permission = "sysUser:page", Type = MenuTypeEnum.Btn, CreateTime = SeedTime, OrderNo = 100 },
            new SysMenu { Id = 1310000000113, Pid = 1310000000111, Title = "编辑", Permission = "sysUser:update", Type = MenuTypeEnum.Btn, CreateTime = SeedTime, OrderNo = 100 },
            new SysMenu { Id = 1310000000114, Pid = 1310000000111, Title = "增加", Permission = "sysUser:add", Type = MenuTypeEnum.Btn, CreateTime = SeedTime, OrderNo = 100 },
            new SysMenu { Id = 1310000000115, Pid = 1310000000111, Title = "删除", Permission = "sysUser:delete", Type = MenuTypeEnum.Btn, CreateTime = SeedTime, OrderNo = 100 },

            new SysMenu { Id = 1310000000121, Pid = 1310000000101, Title = "角色管理", Path = "/system/role", Name = "sysRole", Component = "/system/role/index", Icon = "ele-ColdDrink", Type = MenuTypeEnum.Menu, CreateTime = SeedTime, OrderNo = 110 },
            new SysMenu { Id = 1310000000122, Pid = 1310000000121, Title = "查询", Permission = "sysRole:page", Type = MenuTypeEnum.Btn, CreateTime = SeedTime, OrderNo = 100 },
            new SysMenu { Id = 1310000000123, Pid = 1310000000121, Title = "编辑", Permission = "sysRole:update", Type = MenuTypeEnum.Btn, CreateTime = SeedTime, OrderNo = 100 },
            new SysMenu { Id = 1310000000124, Pid = 1310000000121, Title = "增加", Permission = "sysRole:add", Type = MenuTypeEnum.Btn, CreateTime = SeedTime, OrderNo = 100 },
            new SysMenu { Id = 1310000000125, Pid = 1310000000121, Title = "删除", Permission = "sysRole:delete", Type = MenuTypeEnum.Btn, CreateTime = SeedTime, OrderNo = 100 },
            new SysMenu { Id = 1310000000126, Pid = 1310000000121, Title = "授权菜单", Permission = "sysRole:grantMenu", Type = MenuTypeEnum.Btn, CreateTime = SeedTime, OrderNo = 100 },

            new SysMenu { Id = 1310000000131, Pid = 1310000000101, Title = "机构管理", Path = "/system/org", Name = "sysOrg", Component = "/system/org/index", Icon = "ele-OfficeBuilding", Type = MenuTypeEnum.Menu, CreateTime = SeedTime, OrderNo = 120 },
            new SysMenu { Id = 1310000000141, Pid = 1310000000101, Title = "职位管理", Path = "/system/pos", Name = "sysPos", Component = "/system/pos/index", Icon = "ele-Postcard", Type = MenuTypeEnum.Menu, CreateTime = SeedTime, OrderNo = 130 },
            new SysMenu { Id = 1310000000161, Pid = 1310000000101, Title = "个人中心", Path = "/system/userCenter", Name = "sysUserCenter", Component = "/system/user/component/userCenter", Icon = "ele-Medal", Type = MenuTypeEnum.Menu, CreateTime = SeedTime, OrderNo = 140 },

            new SysMenu { Id = 1310000000301, Pid = 0, Title = "平台管理", Path = "/platform", Name = "platform", Component = "Layout", Icon = "ele-Menu", Type = MenuTypeEnum.Dir, CreateTime = SeedTime, OrderNo = 11000 },
            new SysMenu { Id = 1310000000321, Pid = 1310000000301, Title = "菜单管理", Path = "/platform/menu", Name = "sysMenu", Component = "/system/menu/index", Icon = "ele-Menu", Type = MenuTypeEnum.Menu, CreateTime = SeedTime, OrderNo = 110 },
            new SysMenu { Id = 1310000000331, Pid = 1310000000301, Title = "参数配置", Path = "/platform/config", Name = "sysConfig", Component = "/system/config/index", Icon = "ele-DocumentCopy", Type = MenuTypeEnum.Menu, CreateTime = SeedTime, OrderNo = 120 },
            new SysMenu { Id = 1310000000341, Pid = 1310000000301, Title = "字典管理", Path = "/platform/dict", Name = "sysDict", Component = "/system/dict/index", Icon = "ele-Collection", Type = MenuTypeEnum.Menu, CreateTime = SeedTime, OrderNo = 130 },
            new SysMenu { Id = 1310000000371, Pid = 1310000000301, Title = "缓存管理", Path = "/platform/cache", Name = "sysCache", Component = "/system/cache/index", Icon = "ele-Loading", Type = MenuTypeEnum.Menu, CreateTime = SeedTime, OrderNo = 160 },

            new SysMenu { Id = 1310000000501, Pid = 0, Title = "日志管理", Path = "/log", Name = "log", Component = "Layout", Icon = "ele-DocumentCopy", Type = MenuTypeEnum.Dir, CreateTime = SeedTime, OrderNo = 12000 },
            new SysMenu { Id = 1310000000511, Pid = 1310000000501, Title = "访问日志", Path = "/log/vislog", Name = "sysVisLog", Component = "/system/log/vislog/index", Icon = "ele-Document", Type = MenuTypeEnum.Menu, CreateTime = SeedTime, OrderNo = 100 },
            new SysMenu { Id = 1310000000521, Pid = 1310000000501, Title = "操作日志", Path = "/log/oplog", Name = "sysOpLog", Component = "/system/log/oplog/index", Icon = "ele-Document", Type = MenuTypeEnum.Menu, CreateTime = SeedTime, OrderNo = 110 },
            new SysMenu { Id = 1310000000531, Pid = 1310000000501, Title = "异常日志", Path = "/log/exlog", Name = "sysExLog", Component = "/system/log/exlog/index", Icon = "ele-Document", Type = MenuTypeEnum.Menu, CreateTime = SeedTime, OrderNo = 120 },
        };
    }
}

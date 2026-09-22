using Microsoft.OpenApi.Extensions;

namespace Admin.NET.Core.Service;

/// <summary>
/// 系统用户服务
/// </summary>
[ApiDescriptionSettings(Order = 490)]
public class SysUserService : IDynamicApiController, ITransient
{
    private readonly UserManager _userManager;
    private readonly SqlSugarRepository<SysUser> _sysUserRep;
    private readonly SysOrgService _sysOrgService;
    private readonly SysUserExtOrgService _sysUserExtOrgService;
    private readonly SysUserRoleService _sysUserRoleService;
    private readonly SysConfigService _sysConfigService;

    public SysUserService(UserManager userManager,
        SqlSugarRepository<SysUser> sysUserRep,
        SysOrgService sysOrgService,
        SysUserExtOrgService sysUserExtOrgService,
        SysUserRoleService sysUserRoleService,
        SysConfigService sysConfigService)
    {
        _userManager = userManager;
        _sysUserRep = sysUserRep;
        _sysOrgService = sysOrgService;
        _sysUserExtOrgService = sysUserExtOrgService;
        _sysUserRoleService = sysUserRoleService;
        _sysConfigService = sysConfigService;
    }

    /// <summary>
    /// 获取用户分页列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取用户分页列表")]
    public async Task<SqlSugarPagedList<UserOutput>> Page(PageUserInput input)
    {
        // 获取用户拥有的机构集合
        //var userOrgIdList = await _sysOrgService.GetUserOrgIdList();
        List<long> orgList = null;
        if (input.OrgId > 0)  // 指定机构查询时
        {
            orgList = await _sysOrgService.GetChildIdListWithSelfById(input.OrgId);
            //orgList = _userManager.SuperAdmin ? orgList : orgList.Where(u => userOrgIdList.Contains(u)).ToList();
        }
        //else // 各管理员只能看到自己机构下的用户列表
        //{
        //    orgList = _userManager.SuperAdmin ? null : userOrgIdList;
        //}

        return await _sysUserRep.AsQueryable()
            .LeftJoin<SysOrg>((u, a) => u.OrgId == a.Id)
            .LeftJoin<SysPos>((u, a, b) => u.PosId == b.Id)
            .Where(u => u.AccountType != AccountTypeEnum.SuperAdmin)
            .WhereIF(orgList != null, u => orgList.Contains(u.OrgId))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Account), u => u.Account.Contains(input.Account))
            .WhereIF(!string.IsNullOrWhiteSpace(input.RealName), u => u.RealName.Contains(input.RealName))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Phone), u => u.Phone.Contains(input.Phone))
            .OrderBy(u => u.OrderNo)
            .Select((u, a, b) => new UserOutput
            {
                Id = u.Id,
                OrgName = a.Name,
                PosName = b.Name,
                RoleName = SqlFunc.Subqueryable<SysUserRole>().LeftJoin<SysRole>((m, n) => m.RoleId == n.Id).Where(m => m.UserId == u.Id).SelectStringJoin((m, n) => n.Name, ",")
            }, true)
            .ToPagedListAsync(input.Page, input.PageSize);
    }


    #region 获得单个用户信息
    /// <summary>
    /// 获得单个用户信息
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [DisplayName("获得单个用户信息")]
    public async Task<SysUser> GetUserById(long id)
    {
        var currentUser = await _sysUserRep.AsQueryable().FirstAsync(x => x.Id == id);
        return currentUser;
    }


    #endregion



    #region 只查询综合管理岗的人员 add by cong 20250804

    /// <summary>
    /// 只查询综合管理岗的人员
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("只查询综合管理岗的人员")]
    public async Task<SqlSugarPagedList<UserOutput>> PageZong(PageUserInput input)
    {
        input.PageSize = 999;
        // 获取用户拥有的机构集合
        var userOrgIdList = await _sysOrgService.GetUserOrgIdList();
        List<long> orgList = null;
        if (input.OrgId > 0)  // 指定机构查询时
        {
            orgList = await _sysOrgService.GetChildIdListWithSelfById(input.OrgId);
            orgList = _userManager.SuperAdmin ? orgList : orgList.Where(u => userOrgIdList.Contains(u)).ToList();
        }
        else // 各管理员只能看到自己机构下的用户列表
        {
            orgList = _userManager.SuperAdmin ? null : userOrgIdList;
        }

        return await _sysUserRep.AsQueryable()
            .LeftJoin<SysOrg>((u, a) => u.OrgId == a.Id)
            .LeftJoin<SysPos>((u, a, b) => u.PosId == b.Id)
            .Where(u => u.AccountType != AccountTypeEnum.SuperAdmin)
            .WhereIF(orgList != null, u => orgList.Contains(u.OrgId))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Account), u => u.Account.Contains(input.Account))
            .WhereIF(!string.IsNullOrWhiteSpace(input.RealName), u => u.RealName.Contains(input.RealName))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Phone), u => u.Phone.Contains(input.Phone))
            .OrderBy(u => u.OrderNo)
            .Select((u, a, b) => new UserOutput
            {
                OrgName = a.Name,
                PosName = b.Name,
                RoleName = SqlFunc.Subqueryable<SysUserRole>().LeftJoin<SysRole>((m, n) => m.RoleId == n.Id && n.Name.Contains("产保工程师")).Where(m => m.UserId == u.Id).SelectStringJoin((m, n) => n.Name, ",")
            }, true)
            .ToPagedListAsync(input.Page, input.PageSize);
    }



    #endregion


    #region 送试方/试验甲方 add by cong 20250804


    [DisplayName("送试方/试验甲方")]
    public async Task<SqlSugarPagedList<UserOutput>> PagePartyA(PageUserInput input)
    {
        input.PageSize = 999;
        // 获取用户拥有的机构集合
        var userOrgIdList = await _sysOrgService.GetUserOrgIdList();
        List<long> orgList = null;
        if (input.OrgId > 0)  // 指定机构查询时
        {
            orgList = await _sysOrgService.GetChildIdListWithSelfById(input.OrgId);
            orgList = _userManager.SuperAdmin ? orgList : orgList.Where(u => userOrgIdList.Contains(u)).ToList();
        }
        else // 各管理员只能看到自己机构下的用户列表
        {
            orgList = _userManager.SuperAdmin ? null : userOrgIdList;
        }
        string partyACode = "songshifang";
        long pid = 0;
        var org = await _sysOrgService.GetSysOrg(new OrgInput { Code = partyACode });

        if (org == null)
        {
            return new SqlSugarPagedList<UserOutput>();
        }
        else
        {
            pid = org.Id;
        }
        
        return await _sysUserRep.AsQueryable()
            .LeftJoin<SysOrg>((u, a) => u.OrgId == a.Id).Where((u, a) => a.Pid == pid)
            .LeftJoin<SysPos>((u, a, b) => u.PosId == b.Id)
            .Where(u => u.AccountType != AccountTypeEnum.SuperAdmin)
            .WhereIF(orgList != null, u => orgList.Contains(u.OrgId))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Account), u => u.Account.Contains(input.Account))
            .WhereIF(!string.IsNullOrWhiteSpace(input.RealName), u => u.RealName.Contains(input.RealName))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Phone), u => u.Phone.Contains(input.Phone))
            .OrderBy(u => u.OrderNo)
            .Select((u, a, b) => new UserOutput
            {
                OrgName = a.Name,
                PosName = b.Name,
                RoleName = SqlFunc.Subqueryable<SysUserRole>().LeftJoin<SysRole>((m, n) => m.RoleId == n.Id).Where(m => m.UserId == u.Id).SelectStringJoin((m, n) => n.Name, ",")
            }, true)
            .ToPagedListAsync(input.Page, input.PageSize);
    }



    #endregion


 




    /// <summary>
    /// 获得当前最大排序编号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "MaxIndex"), HttpPost]
    [DisplayName("获得当前最大排序编号")]
    public async Task<long> MaxIndex(PageUserInput input)
    {
        input.Page = 1;
        input.PageSize = int.MaxValue; // 设置为最大值，获取所有数据
        // 获取用户拥有的机构集合
        var userOrgIdList = await _sysOrgService.GetUserOrgIdList();
        List<long> orgList = null;
        if (input.OrgId > 0)  // 指定机构查询时
        {
            orgList = await _sysOrgService.GetChildIdListWithSelfById(input.OrgId);
            orgList = _userManager.SuperAdmin ? orgList : orgList.Where(u => userOrgIdList.Contains(u)).ToList();
        }
        else // 各管理员只能看到自己机构下的用户列表
        {
            orgList = _userManager.SuperAdmin ? null : userOrgIdList;
        }

        var maxOrderNoQuery = _sysUserRep.AsQueryable()
            .LeftJoin<SysOrg>((u, a) => u.OrgId == a.Id)
            .LeftJoin<SysPos>((u, a, b) => u.PosId == b.Id)
            .Where(u => u.AccountType != AccountTypeEnum.SuperAdmin)
            .WhereIF(orgList != null, u => orgList.Contains(u.OrgId))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Account), u => u.Account.Contains(input.Account))
            .WhereIF(!string.IsNullOrWhiteSpace(input.RealName), u => u.RealName.Contains(input.RealName))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Phone), u => u.Phone.Contains(input.Phone));

        // 先判断是否有数据，再获取最大值，避免空集合问题
        var hasData = await maxOrderNoQuery.AnyAsync();
        if (!hasData)
        {
            return 1; // 如果没有数据，返回初始值1
        }
        long maxIndex = await maxOrderNoQuery.MaxAsync(u => u.OrderNo) + 1;
        return maxIndex;
    }


    /// <summary>
    /// 增加用户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [UnitOfWork]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    [DisplayName("增加用户")]
    public async Task<long> AddUser(AddUserInput input)
    {
        //var isExist = await _sysUserRep.AsQueryable().ClearFilter().AnyAsync(u => u.Account == input.Account);
        //if (isExist) throw Oops.Oh(ErrorCodeEnum.D1003);
        await CheckAccount(input);
        if (input.IsCheck)
        {
            return 1;
        }

        var password = await _sysConfigService.GetConfigValue<string>(CommonConst.SysPassword);
        var user = input.Adapt<SysUser>();
        user.Password = CryptogramUtil.Encrypt(password);
        var newUser = await _sysUserRep.AsInsertable(user).ExecuteReturnEntityAsync();
        input.Id = newUser.Id;
        await UpdateRoleAndExtOrg(input);

        return newUser.Id;
    }



    #region 检测账号是否重名
    /// <summary>
    /// 检测用户重名
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [UnitOfWork]
    [ApiDescriptionSettings(Name = "Check"), HttpPost]
    [DisplayName("检测用户重名")]
    public async Task<bool> CheckAccount(AddUserInput input)
    {
        bool result = await _sysUserRep.AsQueryable().ClearFilter().AnyAsync(u => u.Account == input.Account);
        if (result) throw Oops.Oh("账号已存在");
        return result;
    }


    #endregion




    /// <summary>
    /// 更新用户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [UnitOfWork]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    [DisplayName("更新用户")]
    public async Task UpdateUser(UpdateUserInput input)
    {
        if (await _sysUserRep.AsQueryable().ClearFilter().AnyAsync(u => u.Account == input.Account && u.Id != input.Id))
            throw Oops.Oh(ErrorCodeEnum.D1003);

        await _sysUserRep.AsUpdateable(input.Adapt<SysUser>()).IgnoreColumns(true)
            .IgnoreColumns(u => new { u.Password, u.Status }).ExecuteCommandAsync();

        await UpdateRoleAndExtOrg(input);

        // 删除用户机构缓存
        SqlSugarFilter.DeleteUserOrgCache(input.Id, _sysUserRep.Context.CurrentConnectionConfig.ConfigId.ToString());
    }

    /// <summary>
    /// 更新角色和扩展机构
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private async Task UpdateRoleAndExtOrg(AddUserInput input)
    {
        await GrantRole(new UserRoleInput { UserId = input.Id, RoleIdList = input.RoleIdList });

        await _sysUserExtOrgService.UpdateUserExtOrg(input.Id, input.ExtOrgIdList);
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [UnitOfWork]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    [DisplayName("删除用户")]
    public async Task DeleteUser(DeleteUserInput input)
    {
        var user = await _sysUserRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D0009);
        if (user.AccountType == AccountTypeEnum.SuperAdmin)
            throw Oops.Oh(ErrorCodeEnum.D1014);
        if (user.Id == _userManager.UserId)
            throw Oops.Oh(ErrorCodeEnum.D1001);

        await _sysUserRep.DeleteAsync(user);

        // 删除用户角色
        await _sysUserRoleService.DeleteUserRoleByUserId(input.Id);

        // 删除用户扩展机构
        await _sysUserExtOrgService.DeleteUserExtOrgByUserId(input.Id);
    }

    /// <summary>
    /// 查看用户基本信息
    /// </summary>
    /// <returns></returns>
    [DisplayName("查看用户基本信息")]
    public async Task<SysUser> GetBaseInfo()
    {
        return await _sysUserRep.GetFirstAsync(u => u.Id == _userManager.UserId);
    }

    /// <summary>
    /// 根据IC编号获取签名文件Id
    /// </summary>
    /// <returns></returns>
    [DisplayName("根据IC编号获取签名文件Id")]
    public async Task<string> GetSignatureIdByICNumber(string number)
    {
        var sysUser = await _sysUserRep.GetFirstAsync(u => u.ICNumber == number);
        if (sysUser == null)
            throw Oops.Oh("IC卡未关联用户，请先前往个人中心关联IC卡");
        if (string.IsNullOrWhiteSpace(sysUser.Signature))
        {
            throw Oops.Oh($"账号 {sysUser.Account} 未上传签名！请先前往个人中心上传签名！");
        }
        return Path.GetFileNameWithoutExtension(sysUser.Signature);
    }

    /// <summary>
    /// 更新用户基本信息
    /// </summary>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "BaseInfo"), HttpPost]
    [DisplayName("更新用户基本信息")]
    public async Task<int> UpdateBaseInfo(SysUser user)
    {
        user.ICNumber = user.ICNumber?.Trim();
        if (!string.IsNullOrWhiteSpace(user.ICNumber))
        {
            var sysUsers = await _sysUserRep.AsQueryable()
            .Where(x => x.ICNumber == user.ICNumber)
            .Where(x => x.Id != user.Id && x.IsDelete != true)
            .Select(x => x.Account)
            .ToListAsync();

            if (sysUsers.Any())
            {
                throw Oops.Oh($"IC卡已被绑定到账号{string.Join("，", sysUsers)}");
            }
        }

        return await _sysUserRep.AsUpdateable(user)
            .IgnoreColumns(u => new { u.CreateTime, u.Account, u.Password, u.AccountType, u.OrgId, u.PosId }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 设置用户状态
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("设置用户状态")]
    public async Task<int> SetStatus(UserInput input)
    {
        if (_userManager.UserId == input.Id)
            throw Oops.Oh(ErrorCodeEnum.D1026);

        var user = await _sysUserRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D0009);
        if (user.AccountType == AccountTypeEnum.SuperAdmin)
            throw Oops.Oh(ErrorCodeEnum.D1015);

        if (!Enum.IsDefined(typeof(StatusEnum), input.Status))
            throw Oops.Oh(ErrorCodeEnum.D3005);

        // 账号禁用则增加黑名单，账号启用则移除黑名单
        var sysCacheService = App.GetService<SysCacheService>();
        if (input.Status == StatusEnum.Disable)
        {
            sysCacheService.Set($"{CacheConst.KeyBlacklist}{user.Id}", $"{user.RealName}-{user.Phone}");

        }
        else
        {
            sysCacheService.Remove($"{CacheConst.KeyBlacklist}{user.Id}");
        }

        user.Status = input.Status;
        return await _sysUserRep.AsUpdateable(user).UpdateColumns(u => new { u.Status }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 授权用户角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("授权用户角色")]
    public async Task GrantRole(UserRoleInput input)
    {
        //var user = await _sysUserRep.GetFirstAsync(u => u.Id == input.UserId) ?? throw Oops.Oh(ErrorCodeEnum.D0009);
        //if (user.AccountType == AccountTypeEnum.SuperAdmin)
        //    throw Oops.Oh(ErrorCodeEnum.D1022);

        await _sysUserRoleService.GrantUserRole(input);
    }

    /// <summary>
    /// 修改用户密码
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("修改用户密码")]
    public async Task<int> ChangePwd(ChangePwdInput input)
    {
        var user = await _sysUserRep.GetFirstAsync(u => u.Id == _userManager.UserId) ?? throw Oops.Oh(ErrorCodeEnum.D0009);
        if (CryptogramUtil.CryptoType == CryptogramEnum.MD5.ToString())
        {
            if (user.Password != MD5Encryption.Encrypt(input.PasswordOld))
                throw Oops.Oh(ErrorCodeEnum.D1004);
        }
        else
        {
            if (CryptogramUtil.Decrypt(user.Password) != input.PasswordOld)
                throw Oops.Oh(ErrorCodeEnum.D1004);
        }

        // 验证密码强度
        if (CryptogramUtil.StrongPassword)
        {
            user.Password = input.PasswordNew.TryValidate(CryptogramUtil.PasswordStrengthValidation)
                ? CryptogramUtil.Encrypt(input.PasswordNew)
                : throw Oops.Oh(CryptogramUtil.PasswordStrengthValidationMsg);
        }
        else
        {
            user.Password = CryptogramUtil.Encrypt(input.PasswordNew);
        }
        user.PasswordExpireDate = DateTime.Now.AddDays(CryptogramUtil.PasswordValidityPeriod);
        return await _sysUserRep.AsUpdateable(user).UpdateColumns(u => new { u.Password, u.PasswordExpireDate }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 重置用户密码
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("重置用户密码")]
    public async Task<string> ResetPwd(ResetPwdUserInput input)
    {
        var user = await _sysUserRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D0009);
        var password = await _sysConfigService.GetConfigValue<string>(CommonConst.SysPassword);
        user.Password = CryptogramUtil.Encrypt(password);
        await _sysUserRep.AsUpdateable(user).UpdateColumns(u => u.Password).ExecuteCommandAsync();
        return password;
    }

    /// <summary>
    /// 获取用户拥有角色集合
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [DisplayName("获取用户拥有角色集合")]
    public async Task<List<long>> GetOwnRoleList(long userId)
    {
        return await _sysUserRoleService.GetUserRoleIdList(userId);
    }

    /// <summary>
    /// 获取用户扩展机构集合
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [DisplayName("获取用户扩展机构集合")]
    public async Task<List<SysUserExtOrg>> GetOwnExtOrgList(long userId)
    {
        return await _sysUserExtOrgService.GetUserExtOrgList(userId);
    }
}

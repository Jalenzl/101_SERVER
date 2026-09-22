using Furion.SpecificationDocument;


namespace Admin.NET.Core.Service;

/// <summary>
/// 系统登录授权服务
/// </summary>
[ApiDescriptionSettings(Order = 500)]
public class SysAuthService : IDynamicApiController, ITransient
{
    private readonly UserManager _userManager;
    private readonly SqlSugarRepository<SysUser> _sysUserRep;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly SysMenuService _sysMenuService;
    private readonly SysConfigService _sysConfigService;
    private readonly SysCacheService _sysCacheService;

    public SysAuthService(UserManager userManager,
        SqlSugarRepository<SysUser> sysUserRep,
        IHttpContextAccessor httpContextAccessor,
        SysMenuService sysMenuService,
        SysConfigService sysConfigService,
        SysCacheService sysCacheService)
    {
        _userManager = userManager;
        _sysUserRep = sysUserRep;
        _httpContextAccessor = httpContextAccessor;
        _sysMenuService = sysMenuService;
        _sysConfigService = sysConfigService;
        _sysCacheService = sysCacheService;
    }

    /// <summary>
    /// 
    /// </summary>
    [ApiDescriptionSettings(Name = "KeepAlive"), HttpGet]
    [DisplayName("续期请求")]
    public void KeepAlive()
    {

    }

    /// <summary>
    /// 账号密码登录
    /// </summary>
    /// <param name="input"></param>
    /// <remarks>用户名/密码：superadmin/123456</remarks>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("账号密码登录")]
    public async Task<LoginOutput> Login([Required] LoginInput input)
    {
        var result = new LoginOutput();
        var ip = App.HttpContext.GetRemoteIpAddressToIPv4();
        CheckLocaked(ip, input.Account);

        // 账号是否存在
        var user = await _sysUserRep.AsQueryable().Includes(t => t.SysOrg).ClearFilter().FirstAsync(u => u.Account.Equals(input.Account));
        if (user == null)
        {
            RecordFailLogin(ip, input.Account);
            throw Oops.Oh(ErrorCodeEnum.D0009);
        }

        // 账号是否被冻结
        if (user?.Status == StatusEnum.Disable)
            throw Oops.Oh(ErrorCodeEnum.D1017);

        // 租户是否被禁用
        var tenant = await _sysUserRep.ChangeRepository<SqlSugarRepository<SysTenant>>().GetFirstAsync(u => u.Id == user.TenantId);
        if (tenant != null && tenant.Status == StatusEnum.Disable)
            throw Oops.Oh(ErrorCodeEnum.Z1003);

        if (user.PasswordExpireDate == null)
        {
            result.IsDefaultPasswordAndFirstLogin = true;
            user.PasswordExpireDate = DateTime.Now.AddDays(CryptogramUtil.PasswordValidityPeriod);
            await _sysUserRep.AsUpdateable(user).UpdateColumns(u => new { u.PasswordExpireDate }).ExecuteCommandAsync();
        }
        if (CryptogramUtil.EnablePasswordExpire)
        {
            if (user.PasswordExpireDate < DateTime.Now)
            {
                result.IsPasswordExpire = true;
                return result;
            }
                
        }
        // 国密SM2解密（前端密码传输SM2加密后的）
        input.Password = CryptogramUtil.SM2Decrypt(input.Password);

        // 密码是否正确
        if (CryptogramUtil.CryptoType == CryptogramEnum.MD5.ToString())
        {
            if (!user.Password.Equals(MD5Encryption.Encrypt(input.Password)))
            {
                RecordFailLogin(ip, input.Account);
                throw Oops.Oh(ErrorCodeEnum.D1000);
            }
                
        }
        else
        {
            if (!CryptogramUtil.Decrypt(user.Password).Equals(input.Password))
            {
                RecordFailLogin(ip, input.Account);
                throw Oops.Oh(ErrorCodeEnum.D1000);
            }
        }
        CleanFailLogin(ip, input.Account);
        var token = await CreateToken(user);
        result.AccessToken = token.AccessToken;
        result.RefreshToken = token.RefreshToken;
        return result;
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="input"></param>
    /// <remarks>用户名/密码：superadmin/123456</remarks>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("修改密码")]
    public async Task<LoginOutput> ChangePassword([Required] ChangePasswordInput input)
    {
        var ip = App.HttpContext.GetRemoteIpAddressToIPv4();
        CheckLocaked(ip, input.Account);

        // 账号是否存在
        var user = await _sysUserRep.AsQueryable().Includes(t => t.SysOrg).ClearFilter().FirstAsync(u => u.Account.Equals(input.Account));
        if (user == null)
        {
            RecordFailLogin(ip, input.Account);
            throw Oops.Oh(ErrorCodeEnum.D0009);
        }

        // 账号是否被冻结
        if (user?.Status == StatusEnum.Disable)
            throw Oops.Oh(ErrorCodeEnum.D1017);

        // 租户是否被禁用
        var tenant = await _sysUserRep.ChangeRepository<SqlSugarRepository<SysTenant>>().GetFirstAsync(u => u.Id == user.TenantId);
        if (tenant != null && tenant.Status == StatusEnum.Disable)
            throw Oops.Oh(ErrorCodeEnum.Z1003);       

        // 国密SM2解密（前端密码传输SM2加密后的）
        input.Password = CryptogramUtil.SM2Decrypt(input.Password);
        input.NewPassword = CryptogramUtil.SM2Decrypt(input.NewPassword);
        input.ConfirmPassword = CryptogramUtil.SM2Decrypt(input.ConfirmPassword);

        // 密码是否正确
        if (CryptogramUtil.CryptoType == CryptogramEnum.MD5.ToString())
        {
            if (!user.Password.Equals(MD5Encryption.Encrypt(input.Password)))
            {
                RecordFailLogin(ip, input.Account);
                throw Oops.Oh(ErrorCodeEnum.D1000);
            }

        }
        else
        {
            if (!CryptogramUtil.Decrypt(user.Password).Equals(input.Password))
            {
                RecordFailLogin(ip, input.Account);
                throw Oops.Oh(ErrorCodeEnum.D1000);
            }
        }
        CleanFailLogin(ip, input.Account);

        if (input.NewPassword != input.ConfirmPassword)
        {
            throw Oops.Oh("新密码和确认密码不一致！");
        }
        // 验证密码强度
        if (CryptogramUtil.StrongPassword)
        {
            user.Password = input.NewPassword.TryValidate(CryptogramUtil.PasswordStrengthValidation)
                ? CryptogramUtil.Encrypt(input.NewPassword)
                : throw Oops.Oh(CryptogramUtil.PasswordStrengthValidationMsg);
        }
        else
        {
            user.Password = CryptogramUtil.Encrypt(input.NewPassword);
        }
        user.PasswordExpireDate = DateTime.Now.AddDays(CryptogramUtil.PasswordValidityPeriod);
        await _sysUserRep.AsUpdateable(user).UpdateColumns(u => new { u.Password, u.PasswordExpireDate}).ExecuteCommandAsync();
        return await CreateToken(user);
    }

    private void RecordFailLogin(string ip, string account)
    {
        if (!CryptogramUtil.EnableLoginFail)
            return;
        var failLoginIpKey = $"{CacheConst.FailLoginIp}{ip}";
        var failLoginAccountKey = $"{CacheConst.FailLoginAccount}{account}";
        var ipFailCount = _sysCacheService.Increment(failLoginIpKey,1);
        if (ipFailCount == 1)
            _sysCacheService.SetExpire(failLoginIpKey, TimeSpan.FromMinutes(CryptogramUtil.LockMinutes));
        var accountFailCount = _sysCacheService.Increment(failLoginAccountKey,1);
        if (accountFailCount == 1)
            _sysCacheService.SetExpire(failLoginAccountKey, TimeSpan.FromMinutes(CryptogramUtil.LockMinutes));
    }

    private void CleanFailLogin(string ip, string account)
    {
        if (!CryptogramUtil.EnableLoginFail)
            return;
        var failLoginIpKey = $"{CacheConst.FailLoginIp}{ip}";
        var failLoginAccountKey = $"{CacheConst.FailLoginAccount}{account}";
        var ipFailCount = _sysCacheService.Remove(failLoginIpKey);      
        var accountFailCount = _sysCacheService.Remove(failLoginAccountKey);
    }

    private void CheckLocaked(string ip, string account)
    {
        if (!CryptogramUtil.EnableLoginFail)
            return;
        var failLoginIp = _sysCacheService.Get<long>($"{CacheConst.FailLoginIp}{ip}");
        var failLoginAccount = _sysCacheService.Get<long>($"{CacheConst.FailLoginAccount}{account}");
        if (failLoginIp >= CryptogramUtil.FailCount || failLoginAccount >= CryptogramUtil.FailCount)
            throw Oops.Oh(@$"失败次数已达上限，请{CryptogramUtil.LockMinutes}分钟后再试！");

    }

    /// <summary>
    /// IC 卡登录
    /// </summary>
    /// <param name="input"></param>
    /// <remarks>用户名/密码：superadmin/123456</remarks>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("IC卡登录")]
    public async Task<LoginOutput> LoginICCard([Required] LoginICCardInput input)
    {      
        // 账号是否存在
        var user = await _sysUserRep.AsQueryable().Includes(t => t.SysOrg).ClearFilter().FirstAsync(u => u.ICNumber.Equals(input.Number));
        _ = user ?? throw Oops.Oh(ErrorCodeEnum.D0009);

        // 账号是否被冻结
        if (user.Status == StatusEnum.Disable)
            throw Oops.Oh(ErrorCodeEnum.D1017);

        // 租户是否被禁用
        var tenant = await _sysUserRep.ChangeRepository<SqlSugarRepository<SysTenant>>().GetFirstAsync(u => u.Id == user.TenantId);
        if (tenant != null && tenant.Status == StatusEnum.Disable)
            throw Oops.Oh(ErrorCodeEnum.Z1003);

        return await CreateToken(user);
    }


    /// <summary>
    /// 手机号登录
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("手机号登录")]
    public async Task<LoginOutput> LoginPhone([Required] LoginPhoneInput input)
    {
        var verifyCode = _sysCacheService.Get<string>($"{CacheConst.KeyPhoneVerCode}{input.Phone}");
        if (string.IsNullOrWhiteSpace(verifyCode))
            throw Oops.Oh("验证码不存在或已失效，请重新获取！");
        if (verifyCode != input.Code)
            throw Oops.Oh("验证码错误！");

        // 账号是否存在
        var user = await _sysUserRep.AsQueryable().Includes(t => t.SysOrg).ClearFilter().FirstAsync(u => u.Phone.Equals(input.Phone));
        _ = user ?? throw Oops.Oh(ErrorCodeEnum.D0009);

        return await CreateToken(user);
    }

    /// <summary>
    /// 生成Token令牌
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<LoginOutput> CreateToken(SysUser user)
    {
        // 生成Token令牌
        var tokenExpire = await _sysConfigService.GetTokenExpire();
        var accessToken = JWTEncryption.Encrypt(new Dictionary<string, object>
        {
            { ClaimConst.UserId, user.Id },
            { ClaimConst.TenantId, user.TenantId },
            { ClaimConst.Account, user.Account },
            { ClaimConst.RealName, user.RealName },
            { ClaimConst.AccountType, user.AccountType },
            { ClaimConst.OrgId, user.OrgId },
            { ClaimConst.OrgName, user.SysOrg?.Name },
            { ClaimConst.OrgType, user.SysOrg?.Type },
            { ClaimConst.PasswordExpireDate, user.PasswordExpireDate },
        }, tokenExpire);

        // 生成刷新Token令牌
        var refreshTokenExpire = await _sysConfigService.GetRefreshTokenExpire();
        var refreshToken = JWTEncryption.GenerateRefreshToken(accessToken, refreshTokenExpire);

        // 设置响应报文头
        _httpContextAccessor.HttpContext.SetTokensOfResponseHeaders(accessToken, refreshToken);

        // Swagger Knife4UI-AfterScript登录脚本
        // ke.global.setAllHeader('Authorization', 'Bearer ' + ke.response.headers['access-token']);

        return new LoginOutput
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    /// <summary>
    /// 获取登录账号
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取登录账号")]
    public async Task<LoginUserOutput> GetUserInfo()
    {
        var user = await _sysUserRep.GetFirstAsync(u => u.Id == _userManager.UserId) ?? throw Oops.Oh(ErrorCodeEnum.D1011).StatusCode(401);
        // 获取机构
        var org = await _sysUserRep.ChangeRepository<SqlSugarRepository<SysOrg>>().GetFirstAsync(u => u.Id == user.OrgId);
        // 获取职位
        var pos = await _sysUserRep.ChangeRepository<SqlSugarRepository<SysPos>>().GetFirstAsync(u => u.Id == user.PosId);
        // 获取拥有按钮权限集合
        var buttons = await _sysMenuService.GetOwnBtnPermList();

        return new LoginUserOutput
        {
            Id = user.Id,
            Account = user.Account,
            RealName = user.RealName,
            AccountType = user.AccountType,
            Avatar = user.Avatar,
            Address = user.Address,
            Signature = user.Signature,
            OrgId = user.OrgId,
            OrgName = org?.Name,
            OrgType = org?.Type,
            PosName = pos?.Name,
            Buttons = buttons
        };
    }

    /// <summary>
    /// 获取刷新Token
    /// </summary>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    [DisplayName("获取刷新Token")]
    public string GetRefreshToken([FromQuery] string accessToken)
    {
        var refreshTokenExpire = _sysConfigService.GetRefreshTokenExpire().GetAwaiter().GetResult();
        return JWTEncryption.GenerateRefreshToken(accessToken, refreshTokenExpire);
    }

    /// <summary>
    /// 退出系统
    /// </summary>
    [DisplayName("退出系统")]
    public void Logout()
    {
        if (string.IsNullOrWhiteSpace(_userManager.Account))
            throw Oops.Oh(ErrorCodeEnum.D1011);

        _httpContextAccessor.HttpContext.SignoutToSwagger();
    }

    /// <summary>
    /// 获取登录配置
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [SuppressMonitor]
    [DisplayName("获取登录配置")]
    public async Task<dynamic> GetLoginConfig()
    {
        var secondVerEnabled = await _sysConfigService.GetConfigValue<bool>(CommonConst.SysSecondVer);
        var captchaEnabled = await _sysConfigService.GetConfigValue<bool>(CommonConst.SysCaptcha);
        return new { SecondVerEnabled = secondVerEnabled, CaptchaEnabled = captchaEnabled };
    }

    /// <summary>
    /// 获取水印配置
    /// </summary>
    /// <returns></returns>
    [SuppressMonitor]
    [DisplayName("获取水印配置")]
    public async Task<dynamic> GetWatermarkConfig()
    {
        var watermarkEnabled = await _sysConfigService.GetConfigValue<bool>(CommonConst.SysWatermark);
        return new { WatermarkEnabled = watermarkEnabled };
    }

    /// <summary>
    /// 获取验证码
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [SuppressMonitor]
    [DisplayName("获取验证码")]
    [Obsolete("暂不支持验证码")]
    public dynamic GetCaptcha()
    {
        return null;
    }

    /// <summary>
    /// Swagger登录检查
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("/swagger/checkUrl"), NonUnify]
    [DisplayName("Swagger登录检查")]
    public int SwaggerCheckUrl()
    {
        return _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated ? 200 : 401;
    }

    /// <summary>
    /// Swagger登录提交
    /// </summary>
    /// <param name="auth"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("/swagger/submitUrl"), NonUnify]
    [DisplayName("Swagger登录提交")]
    public async Task<int> SwaggerSubmitUrl([FromForm] SpecificationAuth auth)
    {
        try
        {
            _sysCacheService.Set(CommonConst.SysCaptcha, false);

            await Login(new LoginInput
            {
                Account = auth.UserName,
                Password = CryptogramUtil.SM2Encrypt(auth.Password),
            });

            _sysCacheService.Remove(CommonConst.SysCaptcha);

            return 200;
        }
        catch (Exception)
        {
            return 401;
        }
    }
}

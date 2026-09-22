namespace Admin.NET.Core.Service;

/// <summary>
/// 用户登录结果
/// </summary>
public class LoginOutput
{
    /// <summary>
    /// 令牌Token
    /// </summary>
    public string AccessToken { get; set; }

    /// <summary>
    /// 刷新Token
    /// </summary>
    public string RefreshToken { get; set; }

    /// <summary>
    /// 密码是否过期
    /// </summary>
    public bool IsPasswordExpire { get; set; } = false;

    /// <summary>
    /// 是否是默认密码
    /// </summary>
    public bool IsDefaultPasswordAndFirstLogin { get; set; } = false;
}
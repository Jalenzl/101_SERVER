namespace Admin.NET.Core.Service;

/// <summary>
/// 用户登录参数
/// </summary>
public class LoginInput
{
    /// <summary>
    /// 账号
    /// </summary>
    /// <example>admin</example>
    [Required(ErrorMessage = "账号不能为空"), MinLength(2, ErrorMessage = "账号不能少于2个字符")]
    public string Account { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    /// <example>123456</example>
    [Required(ErrorMessage = "密码不能为空"), MinLength(3, ErrorMessage = "密码不能少于3个字符")]
    public string Password { get; set; }

    /// <summary>
    /// 验证码Id
    /// </summary>
    public long CodeId { get; set; }

    /// <summary>
    /// 验证码
    /// </summary>
    public string Code { get; set; }
}

public class LoginPhoneInput
{
    /// <summary>
    /// 手机号码
    /// </summary>
    /// <example>admin</example>
    [Required(ErrorMessage = "手机号码不能为空")]
    [DataValidation(ValidationTypes.PhoneNumber, ErrorMessage = "手机号码不正确")]
    public string Phone { get; set; }

    /// <summary>
    /// 验证码
    /// </summary>
    /// <example>123456</example>
    [Required(ErrorMessage = "验证码不能为空"), MinLength(4, ErrorMessage = "验证码不能少于4个字符")]
    public string Code { get; set; }
}

public class LoginICCardInput
{

    /// <summary>
    /// Ic 卡号
    /// </summary>
    /// <example>123456</example>
    [Required(ErrorMessage = "IC 卡号不能为空")]
    public string Number { get; set; }
}

public class ChangePasswordInput
{
    /// <summary>
    /// 账号
    /// </summary>
    /// <example>admin</example>
    [Required(ErrorMessage = "账号不能为空"), MinLength(2, ErrorMessage = "账号不能少于2个字符")]
    public string Account { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    /// <example>123456</example>
    [Required(ErrorMessage = "密码不能为空")]
    public string Password { get; set; }

    /// <summary>
    /// 新密码
    /// </summary>
    /// <example>123456</example>
    [Required(ErrorMessage = "新密码不能为空")]
    public string NewPassword { get; set; }

    /// <summary>
    /// 确认密码
    /// </summary>
    /// <example>123456</example>
    [Required(ErrorMessage = "确认密码不能为空")]
    public string ConfirmPassword { get; set; }

    /// <summary>
    /// 验证码Id
    /// </summary>
    public long CodeId { get; set; }

    /// <summary>
    /// 验证码
    /// </summary>
    public string Code { get; set; }
}
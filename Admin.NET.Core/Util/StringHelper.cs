namespace Admin.NET.Core;

/// <summary>
/// 字符串帮助类
/// </summary>
public static class StringHelper
{
    //
    // 摘要:
    //     首字母小写写
    //
    // 参数:
    //   input:
    public static string FirstCharToLower(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return input.First().ToString().ToLower() + input.Substring(1);
    }

    //
    // 摘要:
    //     首字母大写
    //
    // 参数:
    //   input:
    public static string FirstCharToUpper(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return input.First().ToString().ToUpper() + input.Substring(1);
    }
}
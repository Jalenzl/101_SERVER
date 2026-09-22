namespace Admin.NET.Application101.Validation;

public static class FileUploadPolicy101
{
    public const long MaxBytes = 100L * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".csv", ".png", ".jpg", ".jpeg"
    };

    public static bool IsAllowed(string clientName) =>
        AllowedExtensions.Contains(Path.GetExtension(Path.GetFileName(clientName)));

    public static string ValidateAndGetExtension(string clientName)
    {
        var safeName = Path.GetFileName(clientName);
        var extension = Path.GetExtension(safeName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(safeName) || !AllowedExtensions.Contains(extension))
            throw new InvalidDataException("不支持的文件类型。");
        return extension;
    }
}

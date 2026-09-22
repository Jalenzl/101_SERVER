namespace Admin.NET.Application101.Configuration;

public sealed class Tcp101FileStorageOptions
{
    public const string SectionName = "Tcp101:FileStorage";

    public string? RootPath { get; set; }
}

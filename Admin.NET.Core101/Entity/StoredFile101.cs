namespace Admin.NET.Core101.Entity;

[SugarTable("t101_stored_file")]
public sealed class StoredFile101 : Entity101Base
{
    [SugarColumn(Length = 256)] public string OriginalName { get; set; } = string.Empty;
    [SugarColumn(Length = 256)] public string StorageName { get; set; } = string.Empty;
    [SugarColumn(Length = 512)] public string RelativePath { get; set; } = string.Empty;
    [SugarColumn(Length = 32)] public string Extension { get; set; } = string.Empty;
    [SugarColumn(Length = 128)] public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    [SugarColumn(Length = 64)] public string Sha256 { get; set; } = string.Empty;
    public long UploaderUserId { get; set; }
    [SugarColumn(Length = 64)] public string UploaderName { get; set; } = string.Empty;
}

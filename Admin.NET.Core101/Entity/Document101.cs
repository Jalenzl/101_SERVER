namespace Admin.NET.Core101.Entity;

[SugarTable("t101_document")]
[SugarIndex("ux_t101_document_code", nameof(Code), OrderByType.Asc, true)]
public sealed class Document101 : Entity101Base
{
    [SugarColumn(Length = 64)] public string Code { get; set; } = string.Empty;
    [SugarColumn(Length = 256)] public string Name { get; set; } = string.Empty;
    [SugarColumn(Length = 64)] public string Type { get; set; } = string.Empty;
    [SugarColumn(Length = 64)] public string Department { get; set; } = string.Empty;
    [SugarColumn(IsNullable = true)] public Guid? AuthorPersonId { get; set; }
    [SugarColumn(IsNullable = true)] public DateOnly? PublishedAt { get; set; }
    [SugarColumn(IsNullable = true)] public Guid? CurrentStoredFileId { get; set; }
}

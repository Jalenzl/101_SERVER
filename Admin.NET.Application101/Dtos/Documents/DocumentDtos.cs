using Admin.NET.Application101.Dtos.Common;

namespace Admin.NET.Application101.Dtos.Documents;

public sealed class DocumentPageQuery : PageQuery
{
    public string? Type { get; set; }
    public string? Department { get; set; }
}

public sealed class DocumentDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public Guid? AuthorPersonId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public DateOnly? PublishedAt { get; set; }
    public Guid? CurrentStoredFileId { get; set; }
}

public class CreateDocumentInput
{
    [Required, RegularExpression(@".*\S.*")] public string Code { get; set; } = string.Empty;
    [Required, RegularExpression(@".*\S.*")] public string Name { get; set; } = string.Empty;
    [Required, RegularExpression(@".*\S.*")] public string Type { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public Guid? AuthorPersonId { get; set; }
    public DateOnly? PublishedAt { get; set; }
}

public sealed class UpdateDocumentInput : CreateDocumentInput;

public sealed record StoredFileDownload101(Stream Stream, string OriginalName, string ContentType);

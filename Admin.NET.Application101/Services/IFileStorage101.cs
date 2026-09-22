namespace Admin.NET.Application101.Services;

public sealed record StoredFileWriteResult(
    string OriginalName,
    string StorageName,
    string RelativePath,
    string Extension,
    string ContentType,
    long Size,
    string Sha256);

public interface IFileStorage101
{
    Task<StoredFileWriteResult> SaveAsync(string clientName, string contentType, Stream content,
        CancellationToken cancellationToken);
    Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken);
    Task DeleteIfExistsAsync(string relativePath, CancellationToken cancellationToken);
}

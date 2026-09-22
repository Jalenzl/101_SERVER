namespace Admin.NET.Application101.Services;

public interface IStoredFileRepository101
{
    Task InsertAsync(StoredFile101 entity, CancellationToken cancellationToken);
    Task<StoredFile101?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task MarkDeletedAsync(StoredFile101 entity, CancellationToken cancellationToken);
    Task<bool> IsReferencedAsync(Guid id, CancellationToken cancellationToken);
}

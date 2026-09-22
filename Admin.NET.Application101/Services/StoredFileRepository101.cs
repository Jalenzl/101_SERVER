namespace Admin.NET.Application101.Services;

public sealed class StoredFileRepository101(SqlSugarRepository<StoredFile101> repository)
    : IStoredFileRepository101, ITransient
{
    public async Task InsertAsync(StoredFile101 entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await repository.InsertAsync(entity);
    }

    public async Task<StoredFile101?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await repository.GetFirstAsync(item => item.Id == id);
    }

    public async Task MarkDeletedAsync(StoredFile101 entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        entity.IsDelete = true;
        entity.UpdateTime = DateTime.UtcNow;
        await repository.AsUpdateable(entity).UpdateColumns(item => new { item.IsDelete, item.UpdateTime }).ExecuteCommandAsync();
    }

    public async Task<bool> IsReferencedAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await repository.Context.Queryable<Document101>().AnyAsync(item => item.CurrentStoredFileId == id) ||
            await repository.Context.Queryable<Device101>().AnyAsync(item =>
                item.CertificateStoredFileId == id || item.MaintenanceStoredFileId == id);
    }
}

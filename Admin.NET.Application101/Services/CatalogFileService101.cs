namespace Admin.NET.Application101.Services;

public sealed class CatalogFileService101(
    SqlSugarRepository<CatalogRecord101> catalogs,
    DocumentFileWriter101 fileWriter,
    IFileStorage101 storage,
    UserManager currentUser) : ITransient
{
    public async Task<Guid> UploadAsync(string key, Guid id, string name, string contentType,
        Stream content, CancellationToken cancellationToken)
    {
        if (key != "training") throw Oops.Oh("此数据表不支持附件。").StatusCode(400);
        var record = await catalogs.GetFirstAsync(item => item.Id == id && item.CatalogKey == key)
            ?? throw Oops.Oh("记录不存在。").StatusCode(404);
        StoredFile101? file = null;
        var database = catalogs.Context;
        try
        {
            await database.Ado.BeginTranAsync();
            file = await fileWriter.WriteAsync(name, contentType, content,
                currentUser.UserId, currentUser.RealName, cancellationToken);
            record.StoredFileId = file.Id;
            record.UpdateTime = DateTime.UtcNow;
            await catalogs.AsUpdateable(record).UpdateColumns(item => new { item.StoredFileId, item.UpdateTime })
                .ExecuteCommandAsync();
            await database.Ado.CommitTranAsync();
            return file.Id;
        }
        catch
        {
            if (database.Ado.Transaction is not null) await database.Ado.RollbackTranAsync();
            if (file is not null) await storage.DeleteIfExistsAsync(file.RelativePath, CancellationToken.None);
            throw;
        }
    }
}

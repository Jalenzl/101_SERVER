namespace Admin.NET.Application101.Services;

public sealed class DeviceFileService101(
    SqlSugarRepository<Device101> devices,
    DocumentFileWriter101 fileWriter,
    IFileStorage101 storage,
    UserManager currentUser) : ITransient
{
    public async Task<Guid> UploadAsync(Guid deviceId, string kind, string name, string contentType,
        Stream content, CancellationToken cancellationToken)
    {
        if (kind is not ("certificate" or "maintenance"))
            throw Oops.Oh("不支持的设备附件类型。").StatusCode(400);
        var device = await devices.GetFirstAsync(item => item.Id == deviceId)
            ?? throw Oops.Oh("记录不存在。").StatusCode(404);
        StoredFile101? file = null;
        var database = devices.Context;
        try
        {
            await database.Ado.BeginTranAsync();
            file = await fileWriter.WriteAsync(name, contentType, content,
                currentUser.UserId, currentUser.RealName, cancellationToken);
            if (kind == "certificate") device.CertificateStoredFileId = file.Id;
            else device.MaintenanceStoredFileId = file.Id;
            device.UpdateTime = DateTime.UtcNow;
            await devices.AsUpdateable(device).UpdateColumns(item => new
            {
                item.CertificateStoredFileId, item.MaintenanceStoredFileId, item.UpdateTime
            }).ExecuteCommandAsync();
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

using System.Security.Cryptography;
using System.Text;
using Admin.NET.Application101.Dtos.Common;
using Admin.NET.Application101.Dtos.Documents;
using Microsoft.Extensions.Logging;

namespace Admin.NET.Application101.Services;

public sealed class DocumentService101(
    SqlSugarRepository<Document101> repository,
    IStoredFileRepository101 storedFiles,
    IFileStorage101 storage,
    DocumentFileWriter101 fileWriter,
    UserManager currentUser,
    ILogger<DocumentService101> logger) : ITransient
{
    public async Task<PageResult<DocumentDto>> PageAsync(DocumentPageQuery input)
    {
        var keyword = input.Keyword?.Trim();
        var query = repository.Context.Queryable<Document101, Person101>((document, author) =>
                new JoinQueryInfos(JoinType.Left, document.AuthorPersonId == author.Id))
            .WhereIF(!string.IsNullOrWhiteSpace(keyword), (document, author) =>
                document.Code.Contains(keyword!) || document.Name.Contains(keyword!) || author.Name.Contains(keyword!))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Type), (document, _) => document.Type == input.Type)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Department), (document, _) => document.Department == input.Department)
            .OrderByDescending((document, _) => document.PublishedAt);
        RefAsync<int> total = 0;
        var items = await query.Select((document, author) => new DocumentDto
        {
            Id = document.Id, Code = document.Code, Name = document.Name, Type = document.Type,
            Department = document.Department, AuthorPersonId = document.AuthorPersonId, AuthorName = author.Name,
            PublishedAt = document.PublishedAt, CurrentStoredFileId = document.CurrentStoredFileId
        }).ToPageListAsync(input.Page, input.PageSize, total);
        return new PageResult<DocumentDto> { Items = items, Page = input.Page, PageSize = input.PageSize, Total = total };
    }

    public async Task<DocumentDto> GetAsync(Guid id)
    {
        var item = await repository.Context.Queryable<Document101, Person101>((document, author) =>
                new JoinQueryInfos(JoinType.Left, document.AuthorPersonId == author.Id))
            .Where((document, _) => document.Id == id)
            .Select((document, author) => new DocumentDto
            {
                Id = document.Id, Code = document.Code, Name = document.Name, Type = document.Type,
                Department = document.Department, AuthorPersonId = document.AuthorPersonId,
                AuthorName = author.Name, PublishedAt = document.PublishedAt,
                CurrentStoredFileId = document.CurrentStoredFileId
            }).FirstAsync();
        return item ?? throw NotFound();
    }

    public async Task<Guid> CreateAsync(CreateDocumentInput input)
    {
        await EnsureCodeUnique(input.Code, null);
        var item = Map(input, new Document101());
        await repository.InsertAsync(item);
        return item.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateDocumentInput input)
    {
        var item = await FindAsync(id);
        await EnsureCodeUnique(input.Code, id);
        Map(input, item); item.UpdateTime = DateTime.UtcNow;
        await repository.AsUpdateable(item).ExecuteCommandAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var item = await FindAsync(id);
        if (await repository.Context.Queryable<TaskDocument101>().AnyAsync(row => row.DocumentId == id))
            throw Oops.Oh("文件记录已被任务引用，不能删除。").StatusCode(409);
        item.IsDelete = true; item.UpdateTime = DateTime.UtcNow;
        await repository.AsUpdateable(item).UpdateColumns(row => new { row.IsDelete, row.UpdateTime }).ExecuteCommandAsync();
    }

    public async Task<Guid> UploadAsync(Guid documentId, string clientName, string contentType, Stream content,
        CancellationToken cancellationToken)
    {
        var document = await FindAsync(documentId);
        StoredFile101? file = null;
        var database = repository.Context;
        try
        {
            await database.Ado.BeginTranAsync();
            file = await fileWriter.WriteAsync(clientName, contentType, content,
                currentUser.UserId, currentUser.RealName, cancellationToken);
            document.CurrentStoredFileId = file.Id;
            document.UpdateTime = DateTime.UtcNow;
            await repository.AsUpdateable(document)
                .UpdateColumns(row => new { row.CurrentStoredFileId, row.UpdateTime }).ExecuteCommandAsync();
            await database.Ado.CommitTranAsync();
            return file.Id;
        }
        catch
        {
            if (database.Ado.Transaction is not null) await database.Ado.RollbackTranAsync();
            if (file is not null)
                await storage.DeleteIfExistsAsync(file.RelativePath, CancellationToken.None);
            throw;
        }
    }

    public async Task<StoredFileDownload101> DownloadAsync(Guid fileId, CancellationToken cancellationToken)
    {
        var file = await storedFiles.GetAsync(fileId, cancellationToken) ?? throw NotFound();
        try
        {
            var stream = await storage.OpenReadAsync(file.RelativePath, cancellationToken);
            return new StoredFileDownload101(stream, file.OriginalName, file.ContentType);
        }
        catch (FileNotFoundException)
        {
            var pathHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(file.RelativePath))).ToLowerInvariant();
            logger.LogWarning("Stored file missing. FileId={FileId}, RelativePathHash={RelativePathHash}", fileId, pathHash);
            throw Oops.Oh("文件不存在。").StatusCode(404);
        }
    }

    public async Task DeleteFileAsync(Guid fileId, CancellationToken cancellationToken)
    {
        var file = await storedFiles.GetAsync(fileId, cancellationToken) ?? throw NotFound();
        if (await storedFiles.IsReferencedAsync(fileId, cancellationToken))
            throw Oops.Oh("文件正在使用，不能删除。").StatusCode(409);
        await storage.DeleteIfExistsAsync(file.RelativePath, cancellationToken);
        await storedFiles.MarkDeletedAsync(file, cancellationToken);
    }

    private async Task<Document101> FindAsync(Guid id) =>
        await repository.GetFirstAsync(item => item.Id == id) ?? throw NotFound();

    private async Task EnsureCodeUnique(string code, Guid? exceptId)
    {
        var value = code.Trim();
        if (await repository.IsAnyAsync(item => item.Code == value && (!exceptId.HasValue || item.Id != exceptId.Value)))
            throw Oops.Oh("文件编号已存在。").StatusCode(409);
    }

    private static Document101 Map(CreateDocumentInput input, Document101 item)
    {
        item.Code = input.Code.Trim(); item.Name = input.Name.Trim(); item.Type = input.Type.Trim();
        item.Department = input.Department.Trim(); item.AuthorPersonId = input.AuthorPersonId;
        item.PublishedAt = input.PublishedAt; return item;
    }

    private static Exception NotFound() => Oops.Oh("记录不存在。").StatusCode(404);
}

public sealed class DocumentFileWriter101(IFileStorage101 storage, IStoredFileRepository101 storedFiles) : ITransient
{
    public async Task<StoredFile101> WriteAsync(string clientName, string contentType, Stream content,
        long uploaderUserId, string uploaderName, CancellationToken cancellationToken)
    {
        var written = await storage.SaveAsync(clientName, contentType, content, cancellationToken);
        try
        {
            var file = new StoredFile101
            {
                OriginalName = written.OriginalName, StorageName = written.StorageName,
                RelativePath = written.RelativePath, Extension = written.Extension,
                ContentType = written.ContentType, Size = written.Size, Sha256 = written.Sha256,
                UploaderUserId = uploaderUserId, UploaderName = uploaderName
            };
            await storedFiles.InsertAsync(file, cancellationToken);
            return file;
        }
        catch
        {
            await storage.DeleteIfExistsAsync(written.RelativePath, CancellationToken.None);
            throw;
        }
    }
}

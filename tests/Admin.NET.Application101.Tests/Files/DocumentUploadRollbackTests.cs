using Admin.NET.Application101.Services;
using Admin.NET.Core101.Entity;
using Xunit;

namespace Admin.NET.Application101.Tests.Files;

public sealed class DocumentUploadRollbackTests
{
    [Fact]
    public async Task MetadataFailure_DeletesJustWrittenPhysicalFile_AndPreservesException()
    {
        var storage = new FakeStorage();
        var writer = new DocumentFileWriter101(storage, new FailingRepository());

        var error = await Assert.ThrowsAsync<MarkerException>(() => writer.WriteAsync(
            "test.pdf", "application/pdf", new MemoryStream("safe"u8.ToArray()), 7, "测试员", CancellationToken.None));

        Assert.Equal("p/only.pdf", storage.DeletedPath);
        Assert.Equal("persistence failed", error.Message);
    }

    private sealed class MarkerException(string message) : Exception(message);

    private sealed class FakeStorage : IFileStorage101
    {
        public string? DeletedPath { get; private set; }
        public Task<StoredFileWriteResult> SaveAsync(string clientName, string contentType, Stream content, CancellationToken cancellationToken) =>
            Task.FromResult(new StoredFileWriteResult(clientName, "only.pdf", "p/only.pdf", ".pdf", contentType, 4, "hash"));
        public Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task DeleteIfExistsAsync(string relativePath, CancellationToken cancellationToken)
        {
            DeletedPath = relativePath;
            return Task.CompletedTask;
        }
    }

    private sealed class FailingRepository : IStoredFileRepository101
    {
        public Task InsertAsync(StoredFile101 entity, CancellationToken cancellationToken) =>
            Task.FromException(new MarkerException("persistence failed"));
        public Task<StoredFile101?> GetAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task MarkDeletedAsync(StoredFile101 entity, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<bool> IsReferencedAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}

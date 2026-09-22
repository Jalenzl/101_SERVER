using System.Buffers;
using System.Security.Cryptography;
using Admin.NET.Application101.Configuration;
using Admin.NET.Application101.Health;
using Admin.NET.Application101.Validation;

namespace Admin.NET.Application101.Services;

public sealed class LocalFileStorage101 : IFileStorage101, ITransient
{
    private readonly string root;

    [ActivatorUtilitiesConstructor]
    public LocalFileStorage101(IOptions<Tcp101FileStorageOptions> options)
        : this(Environment.GetEnvironmentVariable(Tcp101StorageHealthCheck.PathEnvironmentVariable)
            ?? options.Value.RootPath
            ?? throw new InvalidOperationException("TCP101_FILE_STORAGE_PATH is required."))
    {
    }

    public LocalFileStorage101(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath)) throw new ArgumentException("Storage root is required.", nameof(rootPath));
        root = Path.GetFullPath(rootPath);
    }

    public async Task<StoredFileWriteResult> SaveAsync(string clientName, string contentType, Stream content,
        CancellationToken cancellationToken)
    {
        var originalName = Path.GetFileName(clientName);
        var extension = FileUploadPolicy101.ValidateAndGetExtension(originalName);
        if (content.CanSeek && content.Length - content.Position > FileUploadPolicy101.MaxBytes)
            throw new InvalidDataException("文件不能超过 100 MB。");
        var storageName = $"{Guid.NewGuid():N}{extension}";
        var relativePath = Path.Combine(DateTime.UtcNow.ToString("yyyy"), DateTime.UtcNow.ToString("MM"), storageName)
            .Replace(Path.DirectorySeparatorChar, '/');
        var destination = Resolve(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

        var buffer = ArrayPool<byte>.Shared.Rent(81920);
        long size = 0;
        try
        {
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            await using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                buffer.Length, FileOptions.Asynchronous | FileOptions.SequentialScan);
            while (true)
            {
                var read = await content.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                if (read == 0) break;
                size += read;
                if (size > FileUploadPolicy101.MaxBytes)
                    throw new InvalidDataException("文件不能超过 100 MB。");
                hash.AppendData(buffer, 0, read);
                await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            }
            if (size == 0) throw new InvalidDataException("文件不能为空。");
            await output.FlushAsync(cancellationToken);
            return new StoredFileWriteResult(originalName, storageName, relativePath, extension,
                string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
                size, Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant());
        }
        catch
        {
            if (File.Exists(destination)) File.Delete(destination);
            throw;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Stream stream = new FileStream(Resolve(relativePath), FileMode.Open, FileAccess.Read, FileShare.Read,
            81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Task.FromResult(stream);
    }

    public Task DeleteIfExistsAsync(string relativePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = Resolve(relativePath);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    private string Resolve(string relativePath)
    {
        if (Path.IsPathRooted(relativePath)) throw new InvalidDataException("非法文件路径。");
        var fullPath = Path.GetFullPath(Path.Combine(root, relativePath));
        var prefix = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("非法文件路径。");
        return fullPath;
    }
}

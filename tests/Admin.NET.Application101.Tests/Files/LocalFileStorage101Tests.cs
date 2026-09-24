using Admin.NET.Application101.Services;
using Admin.NET.Application101.Validation;
using Admin.NET.Application101.Configuration;
using Admin.NET.Application101.Health;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;

namespace Admin.NET.Application101.Tests.Files;

public sealed class LocalFileStorage101Tests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "tcp101-storage-tests", Guid.NewGuid().ToString("N"));

    [Theory]
    [InlineData("../../appsettings.pdf")]
    [InlineData("..\\..\\web.pdf")]
    [InlineData("C:\\Windows\\win.pdf")]
    public async Task SaveAsync_NeverUsesClientPath(string clientName)
    {
        var storage = new LocalFileStorage101(root);
        await using var content = new MemoryStream("safe"u8.ToArray());
        var saved = await storage.SaveAsync(clientName, "application/pdf", content, CancellationToken.None);
        var rootPrefix = Path.GetFullPath(root) + Path.DirectorySeparatorChar;
        var destination = Path.GetFullPath(Path.Combine(root, saved.RelativePath));
        Assert.StartsWith(rootPrefix, destination, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("..", saved.RelativePath);
        Assert.DoesNotContain("Windows", saved.RelativePath, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("payload.exe", false)]
    [InlineData("report.pdf", true)]
    [InlineData("instruction.docx", true)]
    public void ExtensionPolicy_IsExplicit(string name, bool expected) =>
        Assert.Equal(expected, FileUploadPolicy101.IsAllowed(name));

    [Fact]
    public async Task SaveAsync_RejectsEmptyContent()
    {
        var storage = new LocalFileStorage101(root);
        await Assert.ThrowsAsync<InvalidDataException>(() => storage.SaveAsync(
            "empty.pdf", "application/pdf", new MemoryStream(), CancellationToken.None));
    }

    [Fact]
    public async Task SaveAsync_RejectsMoreThanOneHundredMegabytes()
    {
        var storage = new LocalFileStorage101(root);
        await Assert.ThrowsAsync<InvalidDataException>(() => storage.SaveAsync(
            "large.pdf", "application/pdf", new OversizedStream(), CancellationToken.None));
    }

    [Fact]
    public async Task StorageAndHealthCheck_UseConfiguredRootPath()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Tcp101:FileStorage:RootPath"] = root
            })
            .Build();
        var options = Options.Create(new Tcp101FileStorageOptions());
        var storage = new LocalFileStorage101(options, configuration);
        await using var content = new MemoryStream("safe"u8.ToArray());
        var saved = await storage.SaveAsync("test.pdf", "application/pdf", content, CancellationToken.None);

        Assert.True(File.Exists(Path.Combine(root, saved.RelativePath)));
        var health = new Tcp101StorageHealthCheck(options, configuration);
        var result = await health.CheckHealthAsync(new HealthCheckContext());
        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    public void Dispose()
    {
        if (Directory.Exists(root)) Directory.Delete(root, true);
    }

    private sealed class OversizedStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => FileUploadPolicy101.MaxBytes + 1;
        public override long Position { get; set; }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new InvalidOperationException("Length should be checked first.");
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}

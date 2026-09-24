using Admin.NET.Application101.Configuration;

namespace Admin.NET.Application101.Health;

public sealed class Tcp101StorageHealthCheck(
    IOptions<Tcp101FileStorageOptions> options,
    IConfiguration configuration) : IHealthCheck
{
    public const string PathEnvironmentVariable = "TCP101_FILE_STORAGE_PATH";

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var rootPath = Tcp101SecretProvider.Resolve(configuration,
            PathEnvironmentVariable, $"{Tcp101FileStorageOptions.SectionName}:RootPath")
            ?? options.Value.RootPath;

        if (string.IsNullOrWhiteSpace(rootPath))
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Unhealthy storage"));
        }

        string? probePath = null;
        try
        {
            Directory.CreateDirectory(rootPath);
            probePath = Path.Combine(rootPath, $".tcp101-health-{Guid.NewGuid():N}.probe");
            using (File.Create(probePath))
            {
            }

            File.Delete(probePath);
            return Task.FromResult(HealthCheckResult.Healthy("Healthy"));
        }
        catch
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Unhealthy storage"));
        }
        finally
        {
            if (probePath is not null && File.Exists(probePath))
            {
                try
                {
                    File.Delete(probePath);
                }
                catch
                {
                    // The health result already reports the storage failure.
                }
            }
        }
    }
}

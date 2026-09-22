namespace Admin.NET.Application101.Health;

public sealed class Tcp101DatabaseHealthCheck(ISqlSugarClient database) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            database.Ado.CheckConnection();
            return Task.FromResult(HealthCheckResult.Healthy("Healthy"));
        }
        catch
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Unhealthy database"));
        }
    }
}

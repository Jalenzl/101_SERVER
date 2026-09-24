namespace Admin.NET.Core;

public static class Tcp101SecretProvider
{
    public static string? Resolve(IConfiguration configuration, string environmentVariable, string configurationKey)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var environmentValue = Environment.GetEnvironmentVariable(environmentVariable);
        return string.IsNullOrWhiteSpace(environmentValue) ? configuration[configurationKey] : environmentValue;
    }
}

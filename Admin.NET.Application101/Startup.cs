using Admin.NET.Application101.Configuration;
using Furion;

namespace Admin.NET.Application101;

[AppStartup(90)]
public sealed class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddOptions<Tcp101DatabaseOptions>()
            .Bind(Furion.App.Configuration.GetSection(Tcp101DatabaseOptions.SectionName));
        services.AddOptions<Tcp101FileStorageOptions>()
            .Bind(Furion.App.Configuration.GetSection(Tcp101FileStorageOptions.SectionName));
    }
}

public static class Tcp101ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureTcp101Database(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration.GetSection(Tcp101DatabaseOptions.SectionName)
            .Get<Tcp101DatabaseOptions>() ?? new Tcp101DatabaseOptions();
        var password = Environment.GetEnvironmentVariable(
            Tcp101ConnectionStringFactory.PasswordEnvironmentVariable);
        var connectionString = Tcp101ConnectionStringFactory.Create(options, password);

        configuration["DbConnection:ConnectionConfigs:0:ConnectionString"] = connectionString;
        services.Configure<Tcp101DatabaseOptions>(
            configuration.GetSection(Tcp101DatabaseOptions.SectionName));
        services.Configure<Tcp101FileStorageOptions>(
            configuration.GetSection(Tcp101FileStorageOptions.SectionName));

        return services;
    }
}

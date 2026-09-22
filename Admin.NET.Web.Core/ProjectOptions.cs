using Admin.NET.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Admin.NET.Web.Core;

public static class ProjectOptions
{
    public static IServiceCollection AddProjectOptions(this IServiceCollection services)
    {
        services.AddConfigurableOptions<DbConnectionOptions>();
        services.AddConfigurableOptions<SnowIdOptions>();
        services.AddConfigurableOptions<CacheOptions>();
        services.AddConfigurableOptions<EnumOptions>();
        services.AddConfigurableOptions<CryptogramOptions>();
        return services;
    }
}

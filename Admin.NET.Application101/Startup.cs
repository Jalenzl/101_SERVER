using Admin.NET.Application101.Configuration;
using Admin.NET.Core101.Seed;
using Furion;
using Microsoft.AspNetCore.Builder;

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

    public void Configure(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
        SeedAsync(database).GetAwaiter().GetResult();
    }

    private static async Task SeedAsync(ISqlSugarClient database)
    {
        await InsertWhenEmpty(database, MasterDataSeed101.People);
        await InsertWhenEmpty(database, MasterDataSeed101.Devices);
        await InsertWhenEmpty(database, MasterDataSeed101.StoredFiles);
        await InsertWhenEmpty(database, MasterDataSeed101.Documents);
        await InsertWhenEmpty(database, MasterDataSeed101.WorkflowNodes);
        await InsertWhenEmpty(database, TaskSeed101.Tasks);
        await InsertWhenEmpty(database, TaskSeed101.Plans);
        await InsertWhenEmpty(database, TaskSeed101.TaskPeople);
        await InsertWhenEmpty(database, TaskSeed101.TaskDevices);
        await InsertWhenEmpty(database, TaskSeed101.TaskDocuments);
        await InsertWhenEmpty(database, TaskSeed101.TaskWorkflows);
        await InsertWhenEmpty(database, TaskSeed101.TransferRecords);
    }

    private static async Task InsertWhenEmpty<TEntity>(
        ISqlSugarClient database,
        IReadOnlyList<TEntity> rows) where TEntity : class, new()
    {
        if (rows.Count == 0 || await database.Queryable<TEntity>().AnyAsync())
            return;

        await database.Insertable(rows.ToList()).ExecuteCommandAsync();
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

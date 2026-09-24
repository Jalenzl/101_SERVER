using Admin.NET.Application101.Configuration;
using Admin.NET.Application101.Services;
using Admin.NET.Core101.Seed;
using Furion;
using Microsoft.AspNetCore.Builder;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json.Linq;

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
        await SeedCatalogsAsync(database);
        await SeedPermissionsAsync(database);
    }

    private static async Task InsertWhenEmpty<TEntity>(
        ISqlSugarClient database,
        IReadOnlyList<TEntity> rows) where TEntity : class, new()
    {
        if (rows.Count == 0 || await database.Queryable<TEntity>().AnyAsync())
            return;

        await database.Insertable(rows.ToList()).ExecuteCommandAsync();
    }

    private static async Task SeedPermissionsAsync(ISqlSugarClient database)
    {
        foreach (var menu in MenuSeed101.Menus)
        {
            if (!await database.Queryable<SysMenu>().AnyAsync(item => item.Id == menu.Id))
                await database.Insertable(menu).ExecuteCommandAsync();
        }

        foreach (var roleMenu in RoleMenuSeed101.Items)
        {
            if (!await database.Queryable<SysRoleMenu>().AnyAsync(item => item.Id == roleMenu.Id))
                await database.Insertable(roleMenu).ExecuteCommandAsync();
        }
    }

    private static async Task SeedCatalogsAsync(ISqlSugarClient database)
    {
        using var stream = typeof(Startup).Assembly.GetManifestResourceStream(
            "Admin.NET.Application101.Seed.catalog-seeds.json")
            ?? throw new InvalidOperationException("101 数据字典种子缺失。");
        using var document = await JsonDocument.ParseAsync(stream);
        foreach (var catalog in document.RootElement.EnumerateObject())
        {
            if (!CatalogRecordService101.Keys.Contains(catalog.Name) ||
                await database.Queryable<CatalogRecord101>().AnyAsync(item => item.CatalogKey == catalog.Name))
                continue;
            var rows = catalog.Value.EnumerateArray().Select((value, index) => new CatalogRecord101
            {
                Id = SeedId(catalog.Name, index), CatalogKey = catalog.Name,
                DataJson = JObject.Parse(value.GetRawText())
            }).ToList();
            if (rows.Count > 0) await database.Insertable(rows).ExecuteCommandAsync();
        }
    }

    private static Guid SeedId(string key, int index)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"101:catalog:{key}:{index}"));
        return new Guid(bytes.AsSpan(0, 16));
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
        var password = Tcp101SecretProvider.Resolve(
            configuration,
            Tcp101ConnectionStringFactory.PasswordEnvironmentVariable,
            "Tcp101:Database:Password");
        var connectionString = Tcp101ConnectionStringFactory.Create(options, password);

        configuration["DbConnection:ConnectionConfigs:0:ConnectionString"] = connectionString;
        services.Configure<Tcp101DatabaseOptions>(
            configuration.GetSection(Tcp101DatabaseOptions.SectionName));
        services.Configure<Tcp101FileStorageOptions>(
            configuration.GetSection(Tcp101FileStorageOptions.SectionName));

        return services;
    }
}

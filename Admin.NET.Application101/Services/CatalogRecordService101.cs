using Admin.NET.Application101.Dtos.Common;
using Newtonsoft.Json.Linq;

namespace Admin.NET.Application101.Services;

public sealed record CatalogRecordDto(Guid Id, JObject Data, Guid? StoredFileId);

public sealed class CatalogRecordService101(SqlSugarRepository<CatalogRecord101> repository) : ITransient
{
    public static readonly IReadOnlySet<string> WritableKeys = new HashSet<string>(StringComparer.Ordinal)
    {
        "training", "workload", "messages", "riskLibrarySeverity", "riskLibraryProbability",
        "departments", "areas", "titles", "posts", "subsystems", "testTypes", "engines",
        "codeMappings", "transferMappings", "enums"
    };
    public static readonly IReadOnlySet<string> Keys = new HashSet<string>(WritableKeys.Concat(new[]
    {
        "riskDeviceSeverity", "riskDeviceProbability", "riskDeviceIndex", "riskDeviceCriteria",
        "riskProcessSeverity", "riskProcessOccurrence", "riskProcessDetection",
        "riskLibraryMatrix", "riskLibraryRating"
    }), StringComparer.Ordinal);

    public async Task<PageResult<CatalogRecordDto>> PageAsync(string key, PageQuery query)
    {
        ValidateKey(key);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        RefAsync<int> total = 0;
        var rows = await repository.AsQueryable().Where(item => item.CatalogKey == key)
            .OrderBy(item => item.CreateTime).ToPageListAsync(page, pageSize, total);
        return new PageResult<CatalogRecordDto>
        {
            Items = rows.Select(Map).ToArray(), Total = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<CatalogRecordDto> GetAsync(string key, Guid id)
    {
        ValidateKey(key);
        return Map(await FindAsync(key, id));
    }

    public async Task<Guid> CreateAsync(string key, JObject data)
    {
        ValidateWritableKey(key);
        var item = new CatalogRecord101 { CatalogKey = key, DataJson = Parse(data) };
        await repository.InsertAsync(item);
        return item.Id;
    }

    public async Task UpdateAsync(string key, Guid id, JObject data)
    {
        ValidateWritableKey(key);
        var item = await FindAsync(key, id);
        item.DataJson = Parse(data);
        item.UpdateTime = DateTime.UtcNow;
        await repository.AsUpdateable(item).UpdateColumns(row => new { row.DataJson, row.UpdateTime })
            .ExecuteCommandAsync();
    }

    public async Task DeleteAsync(string key, Guid id)
    {
        ValidateWritableKey(key);
        var item = await FindAsync(key, id);
        item.IsDelete = true;
        item.UpdateTime = DateTime.UtcNow;
        await repository.AsUpdateable(item).UpdateColumns(row => new { row.IsDelete, row.UpdateTime })
            .ExecuteCommandAsync();
    }

    private async Task<CatalogRecord101> FindAsync(string key, Guid id) =>
        await repository.GetFirstAsync(item => item.Id == id && item.CatalogKey == key)
        ?? throw Oops.Oh("记录不存在。").StatusCode(404);

    private static CatalogRecordDto Map(CatalogRecord101 item) =>
        new(item.Id, item.DataJson, item.StoredFileId);

    private static JObject Parse(JObject data)
    {
        if (data is null || data.ToString(Newtonsoft.Json.Formatting.None).Length > 65536 ||
            data.Properties().Any(item => item.Name == "id"))
            throw Oops.Oh("记录格式无效。").StatusCode(400);
        return (JObject)data.DeepClone();
    }

    private static void ValidateKey(string key)
    {
        if (!Keys.Contains(key)) throw Oops.Oh("不支持的数据表。").StatusCode(400);
    }

    private static void ValidateWritableKey(string key)
    {
        ValidateKey(key);
        if (!WritableKeys.Contains(key)) throw Oops.Oh("此数据表只读。").StatusCode(403);
    }
}

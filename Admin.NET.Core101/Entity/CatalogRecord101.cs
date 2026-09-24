using Newtonsoft.Json.Linq;

namespace Admin.NET.Core101.Entity;

[SugarTable("t101_catalog_record")]
[SugarIndex("ix_t101_catalog_record_key", nameof(CatalogKey), OrderByType.Asc)]
public sealed class CatalogRecord101 : Entity101Base
{
    [SugarColumn(Length = 40)] public string CatalogKey { get; set; } = string.Empty;
    [SugarColumn(IsJson = true, ColumnDataType = "jsonb")]
    public JObject DataJson { get; set; } = new();
    [SugarColumn(IsNullable = true)] public Guid? StoredFileId { get; set; }
}

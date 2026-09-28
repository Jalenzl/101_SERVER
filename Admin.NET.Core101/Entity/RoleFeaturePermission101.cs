namespace Admin.NET.Core101.Entity;

[SugarTable("t101_role_feature_permission")]
[SugarIndex("ux_t101_role_feature_permission", nameof(RoleId), OrderByType.Asc,
    nameof(FeatureKey), OrderByType.Asc, true)]
public sealed class RoleFeaturePermission101 : Entity101Base
{
    public long RoleId { get; set; }
    [SugarColumn(Length = 80)] public string FeatureKey { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public bool Editable { get; set; }
    [SugarColumn(Length = 16)] public string DepartmentScope { get; set; } = "unlimited";
    [SugarColumn(ColumnDataType = "text")] public string VisibleFieldsJson { get; set; } = "[]";
}

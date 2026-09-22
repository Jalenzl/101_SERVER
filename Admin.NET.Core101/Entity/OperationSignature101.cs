namespace Admin.NET.Core101.Entity;

[SugarTable("t101_operation_signature")]
[SugarIndex("ux_t101_operation_signature", nameof(Scope), OrderByType.Asc, nameof(ScopeId), OrderByType.Asc,
    nameof(Role), OrderByType.Asc, true)]
public sealed class OperationSignature101 : Entity101Base
{
    [SugarColumn(Length = 32)] public string Scope { get; set; } = string.Empty;
    public Guid ScopeId { get; set; }
    [SugarColumn(Length = 64)] public string Role { get; set; } = string.Empty;
    public long SignerUserId { get; set; }
    [SugarColumn(Length = 64)] public string SignerName { get; set; } = string.Empty;
    public DateTime SignedAt { get; set; }
}

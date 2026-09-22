namespace Admin.NET.Core101.Entity;

public abstract class Entity101Base : IDeletedFilter
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    [SugarColumn(IsNullable = true)]
    public long? CreateUserId { get; set; }

    [SugarColumn(Length = 64, IsNullable = true)]
    public string? CreateUserName { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? UpdateTime { get; set; }

    [SugarColumn(IsNullable = true)]
    public long? UpdateUserId { get; set; }

    [SugarColumn(Length = 64, IsNullable = true)]
    public string? UpdateUserName { get; set; }

    public bool IsDelete { get; set; }
}

namespace Admin.NET.Core101.Entity;

public abstract class Entity101Base : IDeletedFilter
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    public long? CreateUserId { get; set; }

    [SugarColumn(Length = 64)]
    public string? CreateUserName { get; set; }

    public DateTime? UpdateTime { get; set; }
    public long? UpdateUserId { get; set; }

    [SugarColumn(Length = 64)]
    public string? UpdateUserName { get; set; }

    public bool IsDelete { get; set; }
}

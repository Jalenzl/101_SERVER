namespace Admin.NET.Application101.Dtos.Common;

public class PageQuery
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 200)]
    public int PageSize { get; set; } = 20;

    public string? Keyword { get; set; }
}

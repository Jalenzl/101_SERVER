namespace Admin.NET.Core.Service;

/// <summary>
/// 系统操作日志服务
/// </summary>
[ApiDescriptionSettings(Order = 360)]
public class SysLogOpService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SysLogOp> _sysLogOpRep;

    public SysLogOpService(SqlSugarRepository<SysLogOp> sysLogOpRep)
    {
        _sysLogOpRep = sysLogOpRep;
    }

    /// <summary>
    /// 获取操作日志分页列表
    /// </summary>
    /// <returns></returns>
    [SuppressMonitor]
    [DisplayName("获取操作日志分页列表")]
    public async Task<SqlSugarPagedList<SysLogOp>> Page(PageLogInput input)
    {
        return await _sysLogOpRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.StartTime.ToString()), u => u.CreateTime >= input.StartTime)
            .WhereIF(!string.IsNullOrWhiteSpace(input.EndTime.ToString()), u => u.CreateTime <= input.EndTime)
            //.OrderBy(u => u.CreateTime, OrderByType.Desc)
            .OrderBuilder(input)
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 清空操作日志
    /// </summary>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Clear"), HttpPost]
    [DisplayName("清空操作日志")]
    public async Task<bool> Clear()
    {
        return await _sysLogOpRep.DeleteAsync(u => u.Id > 0);
    }

}

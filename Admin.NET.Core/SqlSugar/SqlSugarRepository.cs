namespace Admin.NET.Core;

/// <summary>
/// SqlSugar 实体仓储
/// </summary>
/// <typeparam name="T"></typeparam>
public class SqlSugarRepository<T> : SimpleClient<T> where T : class, new()
{
    protected ITenant iTenant = null;

    public SqlSugarRepository()
    {
        iTenant = App.GetRequiredService<ISqlSugarClient>().AsTenant();
        base.Context = iTenant.GetConnectionScope(SqlSugarConst.MainConfigId);

        // 若实体贴有多库特性，则返回指定库连接
        if (typeof(T).IsDefined(typeof(TenantAttribute), false))
        {
            base.Context = iTenant.GetConnectionScopeWithAttr<T>();
            return;
        }

        // 若实体贴有日志表特性，则返回日志库连接
        if (typeof(T).IsDefined(typeof(LogTableAttribute), false))
        {
            base.Context = iTenant.IsAnyConnection(SqlSugarConst.LogConfigId)
                ? iTenant.GetConnectionScope(SqlSugarConst.LogConfigId)
                : iTenant.GetConnectionScope(SqlSugarConst.MainConfigId);
            return;
        }

        // 若实体贴有系统表特性，则返回默认库连接
        if (typeof(T).IsDefined(typeof(SysTableAttribute), false))
        {
            base.Context = iTenant.GetConnectionScope(SqlSugarConst.MainConfigId);
            return;
        }

        // 101 使用单一 PostgreSQL 数据库，不执行租户级动态数据库切换。
    }

    public virtual async Task<int> InsertOrBulkCopy(List<T> data, int threshold = 1000)
    {
        if (data.Count > threshold)
            return await Context.Fastest<T>().BulkCopyAsync(data);//大数据导入
        else
            return await Context.Insertable(data).ExecuteCommandAsync();//普通导入
    }
}

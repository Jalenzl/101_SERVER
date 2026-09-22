using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Admin.NET.Core.Service;

/// <summary>
/// 系统通用服务
/// </summary>
[ApiDescriptionSettings(Order = 101)]
[AllowAnonymous]
public class SysCommonService : IDynamicApiController, ITransient
{
    private readonly IApiDescriptionGroupCollectionProvider _apiProvider;

    public SysCommonService(IApiDescriptionGroupCollectionProvider apiProvider)
    {
        _apiProvider = apiProvider;
    }

    /// <summary>
    /// 获取所有接口/动态API
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取所有接口/动态API")]
    public List<ApiOutput> GetApiList()
    {
        var apiList = new List<ApiOutput>();
        foreach (var item in _apiProvider.ApiDescriptionGroups.Items)
        {
            foreach (var apiDescription in item.Items)
            {
                var displayName = apiDescription.TryGetMethodInfo(out MethodInfo apiMethodInfo) ? apiMethodInfo.GetCustomAttribute<DisplayNameAttribute>(true)?.DisplayName : "";

                apiList.Add(new ApiOutput
                {
                    GroupName = item.GroupName,
                    DisplayName = displayName,
                    RouteName = apiDescription.RelativePath
                });
            }
        }
        return apiList;
    }
}
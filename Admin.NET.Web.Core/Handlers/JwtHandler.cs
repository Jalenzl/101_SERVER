using Admin.NET.Core;
using Admin.NET.Core.Service;
using Admin.NET.Application101.Authorization;
using Furion;
using Furion.Authorization;
using Furion.DataEncryption;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Admin.NET.Web.Core
{
    public class JwtHandler : AppAuthorizeHandler
    {
        private readonly IServiceProvider _serviceProvider;

        public JwtHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 自动刷新Token
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task HandleAsync(AuthorizationHandlerContext context)
        {
            // var serviceProvider = context.GetCurrentHttpContext().RequestServices;
            using var serviceScope = _serviceProvider.CreateScope();

            // 若当前账号存在黑名单中则授权失败
            var sysCacheService = serviceScope.ServiceProvider.GetService<SysCacheService>();
            if (sysCacheService.ExistKey($"{CacheConst.KeyBlacklist}{context.User.FindFirst(ClaimConst.UserId)?.Value}"))
            {
                context.Fail();
                context.GetCurrentHttpContext().SignoutToSwagger();
                return;
            }

            var sysConfigService = serviceScope.ServiceProvider.GetService<SysConfigService>();
            var tokenExpire = await sysConfigService.GetTokenExpire();
            var refreshTokenExpire = await sysConfigService.GetRefreshTokenExpire();
            if (JWTEncryption.AutoRefreshToken(context, context.GetCurrentHttpContext(), tokenExpire, refreshTokenExpire))
            {
                await AuthorizeHandleAsync(context);
            }
            else
            {
                context.Fail(); // 授权失败
                var currentHttpContext = context.GetCurrentHttpContext();
                if (currentHttpContext == null)
                    return;
                currentHttpContext.SignoutToSwagger();
            }
        }

        public override async Task<bool> PipelineAsync(AuthorizationHandlerContext context, DefaultHttpContext httpContext)
        {
            // 已自动验证 Jwt Token 有效性
            return await CheckAuthorizeAsync(httpContext);
        }

        /// <summary>
        /// 权限校验核心逻辑
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        private static async Task<bool> CheckAuthorizeAsync(DefaultHttpContext httpContext)
        {
            var endpointPermission = httpContext.GetEndpoint()?.Metadata.GetMetadata<ApiPermissionAttribute>()?.Name;
            // 登录模式判断PC、APP
            if (endpointPermission is null &&
                App.User.FindFirst(ClaimConst.LoginMode)?.Value == ((int)LoginModeEnum.APP).ToString())
                return true;

            // 排除超管
            if (App.User.FindFirst(ClaimConst.AccountType)?.Value == ((int)AccountTypeEnum.SuperAdmin).ToString())
                return true;

            // 101 接口优先使用稳定权限名；旧 Admin.NET 接口继续兼容路径推导。
            var routeName = endpointPermission ?? (httpContext.Request.Path.StartsWithSegments("/api")
                ? httpContext.Request.Path.Value![5..].Replace("/", ":")
                : httpContext.Request.Path.Value![1..].Replace("/", ":"));

            if (routeName.Equals("sysAuth:keepAlive", StringComparison.OrdinalIgnoreCase))
                return true;

            // 获取用户拥有按钮权限集合
            var ownBtnPermList = await App.GetService<SysMenuService>().GetOwnBtnPermList();
            // 获取系统所有按钮权限集合
            var allBtnPermList = await App.GetService<SysMenuService>().GetAllBtnPermList();

            return ApiPermissionAuthorization101.IsAllowed(
                routeName, endpointPermission, ownBtnPermList, allBtnPermList);
        }
    }
}

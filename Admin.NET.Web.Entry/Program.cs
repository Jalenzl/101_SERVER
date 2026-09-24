Serve.Run(RunOptions.Default.AddWebComponent<WebComponent>());

public class WebComponent : IWebComponent
{
    public void Load(WebApplicationBuilder builder, ComponentContext componentContext)
    {
        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration.AddUserSecrets(typeof(WebComponent).Assembly, optional: true);
            builder.Configuration.AddEnvironmentVariables();
        }

        // 设置日志过滤
        builder.Logging.AddFilter((provider, category, logLevel) =>
        {
            return !new[] { "Microsoft.Hosting", "Microsoft.AspNetCore" }.Any(u => category.StartsWith(u)) && logLevel >= LogLevel.Information;
        });

        // 设置接口超时时间和上传大小
        builder.WebHost.ConfigureKestrel(u =>
        {
            u.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(30);
            u.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(30);
            u.Limits.MaxRequestBodySize = 101L * 1024 * 1024;
        });
    }
}

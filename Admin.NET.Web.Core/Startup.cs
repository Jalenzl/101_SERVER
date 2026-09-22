using Admin.NET.Core;
using Admin.NET.Core.Service;
using Admin.NET.Application101;
using Admin.NET.Application101.Health;
using Furion;
using Furion.SpecificationDocument;
using IGeekFan.AspNetCore.Knife4jUI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;

namespace Admin.NET.Web.Core;

public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddProjectOptions();
        services.ConfigureTcp101Database(App.Configuration);
        services.AddCache();
        services.AddSqlSugar();
        services.AddJwt<JwtHandler>(enableGlobalAuthorize: true);
        services.AddCorsAccessor();
        services.AddHealthChecks()
            .AddCheck("live", () => HealthCheckResult.Healthy("Healthy"), tags: ["live"])
            .AddCheck<Tcp101DatabaseHealthCheck>("database", tags: ["ready"])
            .AddCheck<Tcp101StorageHealthCheck>("storage", tags: ["ready"]);
        services.AddSpecificationDocuments();

        services.AddControllers()
            .AddAppLocalization()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            })
            .AddInjectWithUnifyResult<AdminResultProvider>();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.All;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        services.AddLoggingSetup();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseForwardedHeaders();
        app.UseUnifyResultStatusCodes();
        app.UseCorsAccessor();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseKnife4UI(options =>
        {
            options.RoutePrefix = "kapi";
            foreach (var groupInfo in SpecificationDocumentBuilder.GetOpenApiGroups())
            {
                options.SwaggerEndpoint("/" + groupInfo.RouteTemplate, groupInfo.Title);
            }
        });

        app.UseInject(string.Empty);
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains("live"),
                ResponseWriter = WriteHealthResponse,
            });
            endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains("ready"),
                ResponseWriter = WriteHealthResponse,
            });
        });
    }

    private static Task WriteHealthResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "text/plain; charset=utf-8";
        var response = "Healthy";
        if (report.Entries.TryGetValue("database", out var database) &&
            database.Status != HealthStatus.Healthy)
        {
            response = "Unhealthy database";
        }
        else if (report.Entries.TryGetValue("storage", out var storage) &&
                 storage.Status != HealthStatus.Healthy)
        {
            response = "Unhealthy storage";
        }

        return context.Response.WriteAsync(response);
    }
}

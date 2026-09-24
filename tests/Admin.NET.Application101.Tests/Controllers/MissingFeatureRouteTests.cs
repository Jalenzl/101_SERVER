using System.Reflection;
using Admin.NET.Application101.Authorization;
using Admin.NET.Application101.Controllers;
using Admin.NET.Application101.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Http.Metadata;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Admin.NET.Application101.Tests.Controllers;

public sealed class MissingFeatureRouteTests
{
    public static TheoryData<Type, string, string, string, string> Routes => new()
    {
        { typeof(TaskRecordsController), "api/101/tasks/{taskId:guid}/records/{kind}", "Get", "GET", "101:task-record:read" },
        { typeof(TaskRecordsController), "api/101/tasks/{taskId:guid}/records/{kind}", "Save", "PUT", "101:task-record:update" },
        { typeof(CatalogRecordsController), "api/101/catalogs/{key}", "Page", "GET", "101:catalog:read" },
        { typeof(CatalogRecordsController), "api/101/catalogs/{key}", "Create", "POST", "101:catalog:write" },
        { typeof(CatalogRecordsController), "api/101/catalogs/{key}", "Upload", "POST", "101:file:upload" },
        { typeof(PreparationSignaturesController), "api/101/tasks/{taskId:guid}/signatures/{section}/{qualifier}", "List", "GET", "101:preparation:signature:read" },
        { typeof(PreparationSignaturesController), "api/101/tasks/{taskId:guid}/signatures/{section}/{qualifier}", "Sign", "POST", "101:preparation:signature:sign" },
        { typeof(PreparationSignaturesController), "api/101/tasks/{taskId:guid}/signatures/{section}/{qualifier}", "Withdraw", "DELETE", "101:preparation:signature:withdraw" },
        { typeof(DevicesController), "api/101/devices", "Upload", "POST", "101:file:upload" },
    };

    [Theory, MemberData(nameof(Routes))]
    public void NewRoute_HasExpectedPermission(Type controller, string path, string actionName,
        string method, string permission)
    {
        Assert.Equal(path, controller.GetCustomAttribute<RouteAttribute>()?.Template);
        var action = controller.GetMethod(actionName)!;
        Assert.Contains(method, action.GetCustomAttributes<HttpMethodAttribute>().Single().HttpMethods);
        Assert.Equal(permission, action.GetCustomAttribute<ApiPermissionAttribute>()?.Name);
    }

    [Fact]
    public void RecordAndCatalogTypes_AreRestrictedToKnownKeys()
    {
        Assert.Equal(8, TaskRecordService101.Kinds.Count);
        Assert.Contains("stops", TaskRecordService101.Kinds);
        Assert.Contains("riskLibrarySeverity", CatalogRecordService101.Keys);
        Assert.DoesNotContain("users", CatalogRecordService101.Keys);
        Assert.DoesNotContain("riskDeviceSeverity", CatalogRecordService101.WritableKeys);
    }

    [Theory]
    [InlineData("{\"restoredAt\":\"2026-09-24\",\"verification\":\"复核完成\"}", true)]
    [InlineData("{\"restoredAt\":\"2026-09-24\",\"verification\":\"\"}", false)]
    [InlineData("{\"reason\":\"故障\"}", true)]
    public void RestoredStop_RequiresVerification(string json, bool expected)
    {
        Assert.Equal(expected, TaskRecordService101.HasRequiredStopVerification(JObject.Parse(json)));
    }

    [Theory]
    [InlineData(typeof(CatalogRecordsController), "Create", 1024 * 1024)]
    [InlineData(typeof(CatalogRecordsController), "Update", 1024 * 1024)]
    [InlineData(typeof(TaskRecordsController), "Save", 16 * 1024 * 1024)]
    [InlineData(typeof(TaskPreparationController), "SaveTransfers", 16 * 1024 * 1024)]
    public void JsonWriteRoutes_LimitRequestBeforeBinding(Type controller, string actionName, long maxBytes)
    {
        var attribute = controller.GetMethod(actionName)!.GetCustomAttribute<RequestSizeLimitAttribute>();
        Assert.Equal(maxBytes, ((IRequestSizeLimitMetadata?)attribute)?.MaxRequestBodySize);
    }
}

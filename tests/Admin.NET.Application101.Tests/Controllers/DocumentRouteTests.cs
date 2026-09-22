using System.Reflection;
using Admin.NET.Application101.Authorization;
using Admin.NET.Application101.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Xunit;

namespace Admin.NET.Application101.Tests.Controllers;

public sealed class DocumentRouteTests
{
    public static TheoryData<Type, string, string, string, string> Routes => new()
    {
        { typeof(DocumentsController), "api/101/documents", nameof(DocumentsController.Page), "GET:", "101:document:read" },
        { typeof(DocumentsController), "api/101/documents", nameof(DocumentsController.Create), "POST:", "101:document:create" },
        { typeof(DocumentsController), "api/101/documents", nameof(DocumentsController.Get), "GET:{id:guid}", "101:document:read" },
        { typeof(DocumentsController), "api/101/documents", nameof(DocumentsController.Update), "PUT:{id:guid}", "101:document:update" },
        { typeof(DocumentsController), "api/101/documents", nameof(DocumentsController.Delete), "DELETE:{id:guid}", "101:document:delete" },
        { typeof(DocumentsController), "api/101/documents", nameof(DocumentsController.Upload), "POST:{id:guid}/files", "101:file:upload" },
        { typeof(FilesController), "api/101/files", nameof(FilesController.Download), "GET:{id:guid}/download", "101:file:download" },
        { typeof(FilesController), "api/101/files", nameof(FilesController.Delete), "DELETE:{id:guid}", "101:file:delete" },
    };

    [Theory, MemberData(nameof(Routes))]
    public void Route_HasExactContract(Type controller, string prefix, string actionName, string route, string permission)
    {
        Assert.NotNull(controller.GetCustomAttribute<ApiControllerAttribute>());
        Assert.Equal(prefix, controller.GetCustomAttribute<RouteAttribute>()?.Template);
        var action = controller.GetMethod(actionName)!;
        var method = action.GetCustomAttributes<HttpMethodAttribute>().Single();
        Assert.Equal(route, $"{method.HttpMethods.Single()}:{method.Template}");
        Assert.Equal(permission, action.GetCustomAttribute<ApiPermissionAttribute>()?.Name);
    }
}

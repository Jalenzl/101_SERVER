using System.Reflection;
using Admin.NET.Application101.Authorization;
using Admin.NET.Application101.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Xunit;

namespace Admin.NET.Application101.Tests.Controllers;

public sealed class MasterDataRouteTests
{
    public static TheoryData<Type, string, string, string, string> Routes => new()
    {
        { typeof(TasksController), "api/101/tasks", nameof(TasksController.Page), "GET:", "101:task:read" },
        { typeof(TasksController), "api/101/tasks", nameof(TasksController.Create), "POST:", "101:task:create" },
        { typeof(TasksController), "api/101/tasks", nameof(TasksController.Get), "GET:{id:guid}", "101:task:read" },
        { typeof(TasksController), "api/101/tasks", nameof(TasksController.Update), "PUT:{id:guid}", "101:task:update" },
        { typeof(TasksController), "api/101/tasks", nameof(TasksController.Delete), "DELETE:{id:guid}", "101:task:delete" },
        { typeof(TasksController), "api/101/tasks", nameof(TasksController.SetStatus), "PUT:{id:guid}/status", "101:task:status" },
        { typeof(PersonnelController), "api/101/personnel", nameof(PersonnelController.Page), "GET:", "101:person:read" },
        { typeof(PersonnelController), "api/101/personnel", nameof(PersonnelController.Create), "POST:", "101:person:create" },
        { typeof(PersonnelController), "api/101/personnel", nameof(PersonnelController.Get), "GET:{id:guid}", "101:person:read" },
        { typeof(PersonnelController), "api/101/personnel", nameof(PersonnelController.Update), "PUT:{id:guid}", "101:person:update" },
        { typeof(PersonnelController), "api/101/personnel", nameof(PersonnelController.Delete), "DELETE:{id:guid}", "101:person:delete" },
        { typeof(DevicesController), "api/101/devices", nameof(DevicesController.Page), "GET:", "101:device:read" },
        { typeof(DevicesController), "api/101/devices", nameof(DevicesController.Create), "POST:", "101:device:create" },
        { typeof(DevicesController), "api/101/devices", nameof(DevicesController.Get), "GET:{id:guid}", "101:device:read" },
        { typeof(DevicesController), "api/101/devices", nameof(DevicesController.Update), "PUT:{id:guid}", "101:device:update" },
        { typeof(DevicesController), "api/101/devices", nameof(DevicesController.Delete), "DELETE:{id:guid}", "101:device:delete" },
        { typeof(WorkflowsController), "api/101/workflows", nameof(WorkflowsController.Page), "GET:", "101:workflow:read" },
        { typeof(WorkflowsController), "api/101/workflows", nameof(WorkflowsController.Tree), "GET:tree", "101:workflow:read" },
        { typeof(WorkflowsController), "api/101/workflows", nameof(WorkflowsController.Create), "POST:", "101:workflow:create" },
        { typeof(WorkflowsController), "api/101/workflows", nameof(WorkflowsController.Update), "PUT:{id:guid}", "101:workflow:update" },
        { typeof(WorkflowsController), "api/101/workflows", nameof(WorkflowsController.Delete), "DELETE:{id:guid}", "101:workflow:delete" },
    };

    [Theory]
    [MemberData(nameof(Routes))]
    public void Route_HasExactContract(Type controller, string prefix, string actionName, string verbAndTemplate, string permission)
    {
        Assert.NotNull(controller.GetCustomAttribute<ApiControllerAttribute>());
        Assert.Equal(prefix, controller.GetCustomAttribute<RouteAttribute>()?.Template);
        var action = controller.GetMethod(actionName)!;
        var method = action.GetCustomAttributes<HttpMethodAttribute>().Single();
        Assert.Equal(verbAndTemplate, $"{method.HttpMethods.Single()}:{method.Template}");
        Assert.Equal(permission, action.GetCustomAttribute<ApiPermissionAttribute>()?.Name);
    }
}

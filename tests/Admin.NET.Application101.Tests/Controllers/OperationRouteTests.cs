using System.Reflection;
using Admin.NET.Application101.Authorization;
using Admin.NET.Application101.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Xunit;

namespace Admin.NET.Application101.Tests.Controllers;

public sealed class OperationRouteTests
{
    public static TheoryData<Type, string, string, string, string> Routes => new()
    {
        { typeof(TasksController), "api/101/tasks", nameof(TasksController.GetWorkflow), "GET:{id:guid}/workflow", "101:workflow-selection:read" },
        { typeof(TasksController), "api/101/tasks", nameof(TasksController.SaveWorkflow), "PUT:{id:guid}/workflow", "101:workflow-selection:update" },
        { typeof(OperationsController), "api/101", nameof(OperationsController.List), "GET:tasks/{taskId:guid}/operations", "101:operation:read" },
        { typeof(OperationsController), "api/101", nameof(OperationsController.Get), "GET:operations/{id:guid}", "101:operation:read" },
        { typeof(OperationsController), "api/101", nameof(OperationsController.Update), "PUT:operations/{id:guid}", "101:operation:update" },
        { typeof(OperationsController), "api/101", nameof(OperationsController.SaveChecks), "PUT:operations/{id:guid}/checks", "101:operation:check" },
        { typeof(OperationsController), "api/101", nameof(OperationsController.Sign), "POST:operations/{id:guid}/signatures", "101:operation:sign" },
        { typeof(OperationsController), "api/101", nameof(OperationsController.WithdrawSignature), "DELETE:operations/{id:guid}/signatures/{role}", "101:operation:withdraw-signature" },
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

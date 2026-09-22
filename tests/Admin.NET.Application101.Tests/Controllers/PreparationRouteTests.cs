using System.Reflection;
using Admin.NET.Application101.Authorization;
using Admin.NET.Application101.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Xunit;

namespace Admin.NET.Application101.Tests.Controllers;

public sealed class PreparationRouteTests
{
    public static TheoryData<string, string, string> Routes => new()
    {
        { nameof(TaskPreparationController.GetPlan), "GET:plan", "101:plan:read" },
        { nameof(TaskPreparationController.SavePlan), "PUT:plan", "101:plan:update" },
        { nameof(TaskPreparationController.GetPreparation), "GET:preparation", "101:preparation:read" },
        { nameof(TaskPreparationController.SavePersonnel), "PUT:personnel", "101:preparation:personnel" },
        { nameof(TaskPreparationController.SaveDevices), "PUT:devices", "101:preparation:device" },
        { nameof(TaskPreparationController.SaveDocuments), "PUT:documents", "101:preparation:document" },
        { nameof(TaskPreparationController.GetTransfers), "GET:transfers/{tableId}", "101:preparation:transfer:read" },
        { nameof(TaskPreparationController.SaveTransfers), "PUT:transfers/{tableId}", "101:preparation:transfer:update" },
    };

    [Theory, MemberData(nameof(Routes))]
    public void Route_HasExactContract(string actionName, string route, string permission)
    {
        var controller = typeof(TaskPreparationController);
        Assert.NotNull(controller.GetCustomAttribute<ApiControllerAttribute>());
        Assert.Equal("api/101/tasks/{taskId:guid}", controller.GetCustomAttribute<RouteAttribute>()?.Template);
        var action = controller.GetMethod(actionName)!;
        var method = action.GetCustomAttributes<HttpMethodAttribute>().Single();
        Assert.Equal(route, $"{method.HttpMethods.Single()}:{method.Template}");
        Assert.Equal(permission, action.GetCustomAttribute<ApiPermissionAttribute>()?.Name);
    }
}

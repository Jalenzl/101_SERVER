using System.Reflection;
using Admin.NET.Application101.Authorization;
using Admin.NET.Application101.Controllers;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace Admin.NET.Application101.Tests.Security;

public sealed class AnonymousAccessTests
{
    [Fact]
    public void OwnFeaturePermissions_RequireLoginWithoutRequiringCatalogRead()
    {
        var action = typeof(RoleFeaturePermissionsController).GetMethod(nameof(RoleFeaturePermissionsController.Mine))!;
        Assert.NotNull(action.GetCustomAttribute<AuthorizeAttribute>());
        Assert.Null(action.GetCustomAttribute<ApiPermissionAttribute>());
    }

    [Theory]
    [InlineData(typeof(TasksController), nameof(TasksController.Page))]
    [InlineData(typeof(PersonnelController), nameof(PersonnelController.Page))]
    [InlineData(typeof(DocumentsController), nameof(DocumentsController.Page))]
    [InlineData(typeof(WorkflowsController), nameof(WorkflowsController.Tree))]
    public void RepresentativeBusinessActions_AreNotAnonymous_AndRequirePermission(Type controller, string actionName)
    {
        var action = controller.GetMethod(actionName)!;
        Assert.Null(controller.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.Null(action.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.NotNull(action.GetCustomAttribute<ApiPermissionAttribute>());
    }
}

using Admin.NET.Application101.Authorization;
using Xunit;

namespace Admin.NET.Application101.Tests.Authorization;

public class ApiPermissionAttributeTests
{
    [Fact]
    public void Constructor_RejectsWhitespaceName()
    {
        Assert.Throws<ArgumentException>(() => new ApiPermissionAttribute("  "));
    }

    [Fact]
    public void Constructor_PreservesStablePermissionName()
    {
        var attribute = new ApiPermissionAttribute("101:task:update");

        Assert.Equal("101:task:update", attribute.Name);
    }

    [Fact]
    public void MarkedEndpoint_DeniesPermissionMissingFromMenus()
    {
        Assert.False(ApiPermissionAuthorization101.IsAllowed(
            "101:catalog:write", "101:catalog:write", [], []));
        Assert.False(ApiPermissionAuthorization101.IsAllowed(
            "101:catalog:write", "101:catalog:write", ["101:catalog:write"], []));
        Assert.True(ApiPermissionAuthorization101.IsAllowed(
            "101:catalog:write", "101:catalog:write", ["101:catalog:write"], ["101:catalog:write"]));
    }

    [Fact]
    public void LegacyEndpoint_KeepsExistingUnknownPermissionBehavior()
    {
        Assert.True(ApiPermissionAuthorization101.IsAllowed("sysConfig:list", null, [], []));
    }
}

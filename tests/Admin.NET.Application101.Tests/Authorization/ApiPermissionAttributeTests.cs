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
}

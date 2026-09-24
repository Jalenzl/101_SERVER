using Admin.NET.Core;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Admin.NET.Application101.Tests.Configuration;

public class Tcp101SecretProviderTests
{
    [Fact]
    public void Resolve_UsesConfigurationWhenEnvironmentVariableIsMissing()
    {
        var variable = $"TCP101_TEST_{Guid.NewGuid():N}";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Tcp101:Database:Password"] = "configured-password"
            })
            .Build();

        Assert.Equal("configured-password", Tcp101SecretProvider.Resolve(
            configuration, variable, "Tcp101:Database:Password"));
    }

    [Fact]
    public void Resolve_PrefersEnvironmentVariableOverConfiguration()
    {
        var variable = $"TCP101_TEST_{Guid.NewGuid():N}";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Tcp101:InitialAdminPassword"] = "configured-password"
            })
            .Build();
        Environment.SetEnvironmentVariable(variable, "environment-password");
        try
        {
            Assert.Equal("environment-password", Tcp101SecretProvider.Resolve(
                configuration, variable, "Tcp101:InitialAdminPassword"));
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable, null);
        }
    }

    [Fact]
    public void Resolve_ReturnsNullWhenNeitherSourceHasAValue()
    {
        var variable = $"TCP101_TEST_{Guid.NewGuid():N}";
        var configuration = new ConfigurationBuilder().Build();

        Assert.Null(Tcp101SecretProvider.Resolve(configuration, variable, "Tcp101:Database:Password"));
    }
}

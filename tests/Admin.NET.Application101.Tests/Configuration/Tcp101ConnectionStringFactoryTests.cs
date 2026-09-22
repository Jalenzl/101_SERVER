using Admin.NET.Application101.Configuration;
using Xunit;

namespace Admin.NET.Application101.Tests.Configuration;

public class Tcp101ConnectionStringFactoryTests
{
    [Fact]
    public void Create_Throws_WhenPasswordIsMissing()
    {
        var options = new Tcp101DatabaseOptions();

        var error = Assert.Throws<InvalidOperationException>(
            () => Tcp101ConnectionStringFactory.Create(options, null));

        Assert.Equal("Environment variable TCP101_DB_PASSWORD is required.", error.Message);
    }

    [Fact]
    public void Create_BuildsExpectedPostgreSqlConnectionString()
    {
        var options = new Tcp101DatabaseOptions { Host = "db", Port = 5432, Username = "app" };
        var password = $"test-{Guid.NewGuid():N}";

        var value = Tcp101ConnectionStringFactory.Create(options, password);
        var parsed = new Npgsql.NpgsqlConnectionStringBuilder(value);

        Assert.Equal("db", parsed.Host);
        Assert.Equal(5432, parsed.Port);
        Assert.Equal("app", parsed.Username);
        Assert.Equal("tcp101", parsed.Database);
        Assert.Equal("public", parsed.SearchPath);
        Assert.Equal(password, parsed.Password);
    }
}

using Npgsql;

namespace Admin.NET.Application101.Configuration;

public static class Tcp101ConnectionStringFactory
{
    public const string PasswordEnvironmentVariable = "TCP101_DB_PASSWORD";

    public static string Create(Tcp101DatabaseOptions options, string? password)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!string.Equals(options.Database, "tpc101", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Tcp101 database name must be tpc101.");
        }

        if (!string.Equals(options.Schema, "public", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Tcp101 database schema must be public.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                $"{PasswordEnvironmentVariable} or Tcp101:Database:Password is required.");
        }

        return new NpgsqlConnectionStringBuilder
        {
            Host = options.Host,
            Port = options.Port,
            Username = options.Username,
            Password = password,
            Database = options.Database,
            SearchPath = options.Schema,
        }.ConnectionString;
    }
}

namespace Admin.NET.Application101.Configuration;

public sealed class Tcp101DatabaseOptions
{
    public const string SectionName = "Tcp101:Database";

    public string Host { get; set; } = "localhost";

    public int Port { get; set; } = 5432;

    public string Username { get; set; } = "postgres";

    public string Database { get; set; } = "tpc101";

    public string Schema { get; set; } = "public";
}

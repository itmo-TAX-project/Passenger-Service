namespace Infrastructure.Db.Options;

public class DatabaseConfigOptions
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    public string Database { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string SslMode { get; set; } = string.Empty;
}
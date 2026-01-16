namespace Infrastructure.Db.Options;

public class DatabaseConfigOptions
{
    public static string SectionName { get; } = "Persistence:DatabaseConfigOptions";

    public string Host { get; set; } = string.Empty;

    public string Port { get; set; } = "5432";

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
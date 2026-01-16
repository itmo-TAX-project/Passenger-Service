using Application.Persistence;
using Infrastructure.Db.Migrations;
using Infrastructure.Db.Options;
using Infrastructure.Db.Repositories;
using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.MessagePersistence;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Extensions;
using Itmo.Dev.Platform.Persistence.Abstractions.Extensions;
using Itmo.Dev.Platform.Persistence.Postgres.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessagePersistence(this IServiceCollection services)
    {
        services.AddUtcDateTimeProvider();
        services.AddSingleton(new Newtonsoft.Json.JsonSerializerSettings());

        services.AddPlatformMessagePersistence(builder => builder
            .WithDefaultPublisherOptions("MessagePersistence:Publisher:Default")
            .UsePostgresPersistence(configurator => configurator.ConfigureOptions("MessagePersistence")));

        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPassengerRepository, PassengerRepository>();

        services.AddPlatformPersistence(persistence => persistence.UsePostgres(postgres =>
            postgres.WithConnectionOptions(builder =>
                {
                    DatabaseConfigOptions options = services.BuildServiceProvider()
                        .GetRequiredService<IOptions<DatabaseConfigOptions>>()
                        .Value;

                    builder.Configure(connectionOptions =>
                    {
                        connectionOptions.Host = options.Host;
                        connectionOptions.Port = Convert.ToInt32(options.Port);
                        connectionOptions.Database = options.Database;
                        connectionOptions.Username = options.Username;
                        connectionOptions.Password = options.Password;
                        connectionOptions.SslMode = options.SslMode;
                    });
                })
                .WithMigrationsFrom(typeof(Initial).Assembly)));

        return services;
    }
}
using Application.Persistence;
using FluentMigrator.Runner;
using Infrastructure.Db.Options;
using Infrastructure.Db.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, DatabaseConfigOptions options)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = options.Host,
            Port = Convert.ToInt32(options.Port),
            Username = options.Username,
            Password = options.Password,
        };
        string connectionString = builder.ToString();

        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .WithMigrationsIn(typeof(ServiceCollectionExtensions).Assembly));

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.ConfigureTypeLoading(connector =>
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        });

        NpgsqlDataSource dataSource = dataSourceBuilder.Build();
        services.AddSingleton(dataSource);

        services.AddScoped<IPassengerRepository, PassengerRepository>();

        return services;
    }
}
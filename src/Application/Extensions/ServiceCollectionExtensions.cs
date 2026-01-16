using Application.Contracts;
using Application.Kafka.InboxHandlers;
using Application.Kafka.Messages.PassengerCreated;
using Application.Services;
using Itmo.Dev.Platform.Kafka.Consumer;
using Itmo.Dev.Platform.Kafka.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafkaApplication(this IServiceCollection collection, IConfiguration configuration)
    {
        collection.AddScoped<IPassengerService, PassengerService>();

        collection.AddInboxConsumer<PassengerCreatedMessageKey, PassengerCreatedMessageValue, PassengerCreatedInboxHandler>(configuration, "Kafka:Consumers:PassengerCreatedMessage");
        return collection;
    }

    internal static IServiceCollection AddInboxConsumer<TKey, TValue, THandler>(this IServiceCollection collection, IConfiguration configuration, string sectionName) where THandler : class, IKafkaInboxHandler<TKey, TValue>
    {
        return collection.AddPlatformKafka(builder => builder
            .ConfigureOptions(configuration.GetSection("Kafka"))
            .AddConsumer(b => b
                .WithKey<TKey>()
                .WithValue<TValue>()
                .WithConfiguration(configuration.GetSection(sectionName))
                .DeserializeKeyWithNewtonsoft()
                .DeserializeValueWithNewtonsoft()
                .HandleInboxWith<THandler>()));
    }
}
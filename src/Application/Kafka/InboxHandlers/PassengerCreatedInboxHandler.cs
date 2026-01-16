using Application.Kafka.Messages.PassengerCreated;
using Application.Models;
using Application.Persistence;
using Itmo.Dev.Platform.Kafka.Consumer;

namespace Application.Kafka.InboxHandlers;

public class PassengerCreatedInboxHandler : IKafkaInboxHandler<PassengerCreatedMessageKey, PassengerCreatedMessageValue>
{
    private readonly IPassengerRepository _passengerRepository;

    public PassengerCreatedInboxHandler(IPassengerRepository passengerRepository)
    {
        _passengerRepository = passengerRepository;
    }

    public async ValueTask HandleAsync(IEnumerable<IKafkaInboxMessage<PassengerCreatedMessageKey, PassengerCreatedMessageValue>> messages, CancellationToken cancellationToken)
    {
        foreach (IKafkaInboxMessage<PassengerCreatedMessageKey, PassengerCreatedMessageValue> message in messages)
        {
            var segments = new AllowedSegments(true, true, true);
            await _passengerRepository.AddPassengerAsync(new Passenger(message.Value.Name, message.Value.Phone, segments), cancellationToken);
        }
    }
}
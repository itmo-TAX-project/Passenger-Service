using Application.Models;

namespace Application.Persistence;

public record PassengerPaginatedResponse(IEnumerable<Passenger>? Accounts, long? PageToken);
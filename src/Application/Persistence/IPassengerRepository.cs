using Application.Models;
using Application.Persistence.SearchFilters;

namespace Application.Persistence;

public interface IPassengerRepository
{
    Task<bool> AddPassengerAsync(Passenger passenger, CancellationToken cancellationToken);

    Task UpdatePassengerAsync(Passenger passenger, CancellationToken cancellationToken);

    Task<Passenger?> GetPassengerBySearchFilterAsync(PassengerSearchFilter passengerSearchFilter, PaginatedRequest paginatedRequest, CancellationToken cancellationToken);

    Task<AllowedSegments?> GetPassengerPreferencesByIdAsync(long id, CancellationToken cancellationToken);

    Task<PassengerPaginatedResponse> GetPassengersByPreferenceSearchFilterAsync(PreferenceSearchFilter preferenceSearchFilter, PaginatedRequest paginatedRequest, CancellationToken cancellationToken);
}
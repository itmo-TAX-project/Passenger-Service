using Application.Dto;
using Application.Models;

namespace Application.Contracts;

public interface IPassengerService
{
    Task UpdatePassengerDetailsAsync(Passenger passenger, CancellationToken cancellationToken);

    Task<Passenger?> GetPassengerAsync(long id, CancellationToken cancellationToken);

    Task<IEnumerable<AllowedSegmentsDto>> GetAllowedSegmentsAsyncById(long id, CancellationToken cancellationToken);
}
using Application.Contracts;
using Application.Dto;
using Application.Models;
using Application.Persistence;
using Application.Persistence.SearchFilters;

namespace Application.Services;

public class PassengerService : IPassengerService
{
    private readonly IPassengerRepository _passengerRepository;

    public PassengerService(IPassengerRepository passengerRepository)
    {
        _passengerRepository = passengerRepository;
    }

    public async Task UpdatePassengerDetailsAsync(Passenger passenger, CancellationToken cancellationToken)
    {
        await _passengerRepository.UpdatePassengerAsync(passenger, cancellationToken);
    }

    public async Task<Passenger?> GetPassengerAsync(long id, CancellationToken cancellationToken)
    {
        var filter = new PassengerSearchFilter(id, null, null);
        var pagination = new PaginatedRequest(1, null);
        return await _passengerRepository.GetPassengerBySearchFilterAsync(filter, pagination, cancellationToken);
    }

    public async Task<IEnumerable<AllowedSegmentsDto>> GetAllowedSegmentsAsyncById(long id, CancellationToken cancellationToken)
    {
        AllowedSegments? preferences = await _passengerRepository.GetPassengerPreferencesByIdAsync(id, cancellationToken);

        if (preferences == null) throw new NullReferenceException("Passenger preferences not found");

        var segments = new List<AllowedSegmentsDto>();
        if (preferences.Basic)
            segments.Add(new AllowedSegmentsDto(id, VehicleSegmentDto.Basic));
        if (preferences.Mid)
            segments.Add(new AllowedSegmentsDto(id, VehicleSegmentDto.Mid));
        if (preferences.Premium)
            segments.Add(new AllowedSegmentsDto(id, VehicleSegmentDto.Premium));
        return segments;
    }
}
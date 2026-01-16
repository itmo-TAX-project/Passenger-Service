using Application.Contracts;
using Application.Dto;
using Grpc.Core;
using PassengerMaster.Grpc;
using Presentation.Grpc.Mapper;

namespace Presentation.Grpc.Service;

public class GrpcPassengerService : PassengerService.PassengerServiceBase
{
    private readonly IPassengerService _passengerService;

    public GrpcPassengerService(IPassengerService passengerService)
    {
        _passengerService = passengerService;
    }

    public override async Task<GetPassengerResponse> GetPassenger(GetPassengerRequest request, ServerCallContext context)
    {
        Application.Models.Passenger? passenger = await _passengerService.GetPassengerAsync(request.AccountId, context.CancellationToken);

        if (passenger == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Not found"));
        }

        IEnumerable<AllowedSegmentsDto> vehicleSegments = await _passengerService
            .GetAllowedSegmentsAsyncById(passenger.PassengerId ?? throw new NullReferenceException(), context.CancellationToken);

        return GrpcMapper.ToGrpcResponse(passenger, vehicleSegments);
    }
}
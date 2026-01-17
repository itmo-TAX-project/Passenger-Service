using AccountMaster.Grpc;
using Application.Contracts;
using Application.Dto;
using Grpc.Core;
using PassengerMaster.Grpc;
using Presentation.Grpc.Mapper;

namespace Presentation.Grpc.Service;

public class GrpcPassengerService : PassengerService.PassengerServiceBase
{
    private readonly IPassengerService _passengerService;

    private readonly AccountService.AccountServiceClient _accountServiceClient;

    public GrpcPassengerService(IPassengerService passengerService, AccountService.AccountServiceClient accountServiceClient)
    {
        _passengerService = passengerService;
        _accountServiceClient = accountServiceClient;
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

    public override async Task<GetAccountIdResponse> GetAccountId(GetAccountIdRequest request, ServerCallContext context)
    {
        Application.Models.Passenger? passenger = await _passengerService.GetPassengerAsync(request.PassengerId, context.CancellationToken);

        if (passenger == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Not found"));
        }

        var accountServiceRequest = new GetAccountIdByNameRequest { Name = passenger.Name };
        GetAccountIdByNameResponse result = await _accountServiceClient.GetAccountIdByNameAsync(accountServiceRequest);

        return new GetAccountIdResponse { AccountId = result.AccountId };
    }
}
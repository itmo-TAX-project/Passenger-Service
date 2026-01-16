using Application.Dto;
using PassengerMaster.Grpc;
using Passenger = PassengerMaster.Grpc.Passenger;

namespace Presentation.Grpc.Mapper;

public static class GrpcMapper
{
    public static GetPassengerResponse ToGrpcResponse(
        Application.Models.Passenger passenger, IEnumerable<Application.Dto.AllowedSegmentsDto> allowedSegments)
    {
        if (passenger.PassengerId == null) throw new NullReferenceException("passenger.PassengerId is null which should not happen");

        var grpcPassenger = new Passenger()
        {
            PassengerId = passenger.PassengerId.Value,
            Name = passenger.Name,
        };

        grpcPassenger.AllowedSegments.AddRange(allowedSegments.Select(MapSegment));

        return new GetPassengerResponse()
        {
            Passenger = grpcPassenger,
        };
    }

    private static VehicleSegment MapSegment(AllowedSegmentsDto segment)
    {
        return segment.Segment switch
        {
            VehicleSegmentDto.Basic => VehicleSegment.Basic,
            VehicleSegmentDto.Mid => VehicleSegment.Mid,
            VehicleSegmentDto.Premium => VehicleSegment.Premium,
            _ => VehicleSegment.Unspecified,
        };
    }
}
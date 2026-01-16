namespace Application.Dto;

public class AllowedSegmentsDto
{
    public AllowedSegmentsDto(long id, VehicleSegmentDto segment)
    {
        PassengerId = id;
        Segment = segment;
    }

    public long PassengerId { get; init; }

    public VehicleSegmentDto Segment { get; set; }
}
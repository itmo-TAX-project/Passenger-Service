namespace Application.Models;

public class Passenger
{
    public Passenger(string name, string phone, AllowedSegments segments, long? passengerId = null)
    {
        PassengerId = passengerId;
        Name = name;
        Phone = phone;
        Segments = segments;
    }

    public long? PassengerId { get; init; }

    public string Name { get; init; }

    public string Phone { get; init; }

    public AllowedSegments Segments { get; set; }
}
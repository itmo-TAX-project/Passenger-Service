namespace Application.Models;

public class AllowedSegments
{
    public AllowedSegments(bool basic, bool mid, bool premium)
    {
        Basic = basic;
        Mid = mid;
        Premium = premium;
    }

    public long? PassengerId { get; set; }

    public bool Basic { get; set; }

    public bool Mid { get; set; }

    public bool Premium { get; set; }
}
namespace CompatibilityApp.Domain.DayFiance.Ratings;

public class RatingTypeDto
{
    public int RatingTypeId { get; set; }
    public string RatingName { get; set; } = "";
    public decimal RatingWeight { get; set; }
    public string RatingCategory { get; set; } = "";
}


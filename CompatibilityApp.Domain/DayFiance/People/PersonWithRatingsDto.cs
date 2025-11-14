namespace CompatibilityApp.Domain.DayFiance.People;

/// <summary>Projection for the grid: RatingsByTypeId maps RatingType.Id -> rating (nullable).</summary>
public class PersonWithRatingsDto : PersonSeasonDto
{

    public IReadOnlyDictionary<int, decimal?> RatingsByTypeId { get; init; } =
        new Dictionary<int, decimal?>();
}

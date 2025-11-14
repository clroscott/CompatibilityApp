namespace CompatibilityApp.Domain.DayFiance.Relationships;

/// <summary>
/// Relationship + season + rating values by RatingTypeId.
/// Mirrors PersonWithRatingsDto.
/// </summary>
public class RelationshipWithRatingsDto : RelationshipSeasonDto
{

    public IReadOnlyDictionary<int, decimal?> RatingsByTypeId { get; init; }
        = new Dictionary<int, decimal?>();
}

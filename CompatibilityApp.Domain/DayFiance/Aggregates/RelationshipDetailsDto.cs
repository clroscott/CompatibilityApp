namespace CompatibilityApp.Domain.DayFiance.Aggregates;

using CompatibilityApp.Domain.DayFiance.People;
using CompatibilityApp.Domain.DayFiance.Relationships;
using CompatibilityApp.Domain.DayFiance.Ratings;

/// <summary>
/// Aggregate read-model representing one relationship plus both people
/// and the rating types used to interpret their ratings.
/// </summary>
public sealed class RelationshipDetailsDto
{
    /// <summary>
    /// Relationship + its ratings (what you already use on the relationship ratings grid).
    /// </summary>
    public RelationshipWithRatingsDto Relationship { get; init; } = default!;

    /// <summary>
    /// First person in the relationship, including ratings.
    /// </summary>
    public PersonWithRatingsDto Person1 { get; init; } = default!;

    /// <summary>
    /// Second person in the relationship, including ratings.
    /// </summary>
    public PersonWithRatingsDto Person2 { get; init; } = default!;

    /// <summary>
    /// Rating type definitions (Id, name, weight, category).
    /// Used by calculation services to understand weights.
    /// </summary>
    public IReadOnlyList<RatingTypeDto> RatingTypes { get; init; } = Array.Empty<RatingTypeDto>();
}

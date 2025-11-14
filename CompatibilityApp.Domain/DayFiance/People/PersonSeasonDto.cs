namespace CompatibilityApp.Domain.DayFiance.People;

/// <summary>
/// Person projected onto a season roster.
/// Inherits shared person fields from PersonDto.
/// </summary>
public class PersonSeasonDto : PersonDto
{
    public short Season { get; init; }

    /// <summary>
    /// Relationship partner for this person on this season (from Relationship/RelationshipSeason).
    /// Null when none assigned.
    /// Made settable because service populates this after projection.
    /// </summary>
    public int? PartnerPersonId { get; set; }
}
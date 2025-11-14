namespace CompatibilityApp.Domain.DayFiance.Relationships;

public class RelationshipDto
{

    public int RelationshipId { get; init; }

    public int PersonId1 { get; init; }
    public int PersonId2 { get; init; }

    public string RelationshipType { get; init; } = "";

    public string ImagePath { get; init; } = "";

    // Optional but very handy for UI:
    public string Person1FirstName { get; init; } = "";
    public string Person1LastName { get; init; } = "";
    public string Person2FirstName { get; init; } = "";
    public string Person2LastName { get; init; } = "";

    public string Person1ImagePath { get; init; } = "";
    public string Person2ImagePath { get; init; } = "";

    public string CoupleDisplayName =>
        $"{Person1FirstName} {Person1LastName}".Trim() +
        " & " +
        $"{Person2FirstName} {Person2LastName}".Trim();
}

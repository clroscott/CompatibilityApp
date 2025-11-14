using CompatibilityApp.Domain.DayFiance.Relationships;

public interface IRelationshipService
{
    Task<IReadOnlyList<RelationshipWithRatingsDto>> GetRelationshipsWithRatingsAsync(
        short? season = null,
        CancellationToken ct = default);

    Task UpsertRelationshipRatingAsync(
        int relationshipId,
        int ratingTypeId,
        decimal? rating,
        short? season = null,
        CancellationToken ct = default);

    Task RemoveRelationshipRatingAsync(
        int relationshipId,
        int ratingTypeId,
        short? season = null,
        CancellationToken ct = default);

    // NEW: store image path
    Task UpdateRelationshipImagePathAsync(
        int relationshipId,
        string imagePath,
        CancellationToken ct = default);
}

namespace CompatibilityApp.Domain.DayFiance.Ratings;

public interface IRatingTypeService
{
    Task<IReadOnlyList<RatingTypeDto>> GetAllAsync(CancellationToken ct = default);
    Task<int> AddAsync(string name, decimal ratingWeight, string category, CancellationToken ct = default);
    Task UpdateAllAsync(int ratingTypeId, RatingTypeDto updatedRating, CancellationToken ct = default);
    Task UpdateWeightAsync(int ratingTypeId, decimal newWeight, CancellationToken ct = default);

    Task RenameAsync(int ratingTypeId, string newName, CancellationToken ct = default);
    Task DeleteAsync(int ratingTypeId, CancellationToken ct = default);
}

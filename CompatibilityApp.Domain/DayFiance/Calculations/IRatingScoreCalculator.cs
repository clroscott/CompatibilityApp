using CompatibilityApp.Domain.DayFiance.Ratings;

namespace CompatibilityApp.Domain.DayFiance.Calculations;

public interface IRatingScoreCalculator
{
    RatingScoreResult Calculate(
        IReadOnlyDictionary<int, decimal?> ratingsByTypeId,
        IReadOnlyList<RatingTypeDto> ratingTypes);
}

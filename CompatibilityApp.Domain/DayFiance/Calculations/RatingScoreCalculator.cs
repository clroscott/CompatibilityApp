using CompatibilityApp.Domain.DayFiance.Ratings;

namespace CompatibilityApp.Domain.DayFiance.Calculations;

public sealed class RatingScoreCalculator : IRatingScoreCalculator
{
    public RatingScoreResult Calculate(
        IReadOnlyDictionary<int, decimal?> ratingsByTypeId,
        IReadOnlyList<RatingTypeDto> ratingTypes)
    {
        decimal totalWeighted = 0m;
        decimal totalWeight = 0m;

        foreach (var rt in ratingTypes)
        {
            if (!ratingsByTypeId.TryGetValue(rt.RatingTypeId, out var rating) || rating is null)
                continue;

            var weight = rt.RatingWeight;

            totalWeighted += rating.Value * weight;
            totalWeight += weight;
        }

        decimal? score = totalWeight == 0m
            ? null
            : totalWeighted / totalWeight;

        var weights = ratingTypes.ToDictionary(
            x => x.RatingTypeId,
            x => x.RatingWeight);

        return new RatingScoreResult(score, ratingsByTypeId, weights);
    }
}

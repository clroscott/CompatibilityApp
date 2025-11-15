namespace CompatibilityApp.Domain.DayFiance.Calculations;

public sealed record RatingScoreResult(
    decimal? Score,
    IReadOnlyDictionary<int, decimal?> Ratings,
    IReadOnlyDictionary<int, decimal> Weights);

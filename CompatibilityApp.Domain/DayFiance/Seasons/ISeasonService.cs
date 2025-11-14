namespace CompatibilityApp.Domain.DayFiance.Seasons;

public interface ISeasonService
{
    Task<IReadOnlyList<short>> GetAllSeasonsAsync(CancellationToken ct = default);
    Task<SeasonDto?> GetSeasonAsync(short season, CancellationToken ct = default);
    Task UpsertSeasonAsync(SeasonDto dto, CancellationToken ct = default);
}

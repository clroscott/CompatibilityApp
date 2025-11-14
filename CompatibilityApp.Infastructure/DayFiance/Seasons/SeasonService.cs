using AutoMapper;
using CompatibilityApp.Domain.DayFiance.Seasons;
using CompatibilityApp.Infrastructure.Data;
using CompatibilityApp.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace CompatibilityApp.Infrastructure.DayFiance.Seasons;

public sealed class SeasonService : ISeasonService
{
    private readonly CompatibiltyDBContext _db;
    private readonly IMapper _mapper;

    public SeasonService(CompatibiltyDBContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<short>> GetAllSeasonsAsync(CancellationToken ct = default)
    {
        var seasons = await _db.Set<Season>()
            .AsNoTracking()
            .Select(s => s.SeasonNum)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync(ct);

        return seasons;
    }

    public async Task<SeasonDto?> GetSeasonAsync(short season, CancellationToken ct = default)
    {
        var entity = await _db.Set<Season>()
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.SeasonNum == season, ct);

        if (entity is null)
            return null;

        return _mapper.Map<SeasonDto>(entity);
    }

    public async Task UpsertSeasonAsync(SeasonDto dto, CancellationToken ct = default)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        // Keep your original assumption: dates must be provided
        if (dto.AirDate is null || dto.FilmingDate is null)
            throw new InvalidOperationException("AirDate and FilmingDate must be provided for a Season.");

        var entity = await _db.Set<Season>()
            .FindAsync(new object?[] { dto.SeasonNum }, ct);

        if (entity is null)
        {
            // Insert: map DTO -> new entity
            entity = _mapper.Map<Season>(dto);
            _db.Set<Season>().Add(entity);
        }
        else
        {
            // Update: map onto existing entity
            _mapper.Map(dto, entity);
            _db.Set<Season>().Update(entity);
        }

        await _db.SaveChangesAsync(ct);
    }
}

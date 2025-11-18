using CompatibilityApp.Domain.DayFiance.Ratings;
using CompatibilityApp.Infrastructure.Data;
using CompatibilityApp.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace CompatibilityApp.Infrastructure.DayFiance.Ratings;

public sealed class RatingTypeService : IRatingTypeService
{
    private readonly CompatibilityDBContext _db;
    private readonly IMapper _mapper;

    public RatingTypeService(CompatibilityDBContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<RatingTypeDto>> GetAllAsync(CancellationToken ct = default)
    {
        var entities = await _db.Set<RatingType>()
            .AsNoTracking()
            .OrderBy(rt => rt.RatingName)
            .ToListAsync(ct);

        return _mapper.Map<List<RatingTypeDto>>(entities);
    }

    public async Task<int> AddAsync(string name, decimal ratingWeight, string category, CancellationToken ct = default)
    {
        var rt = new RatingType
        {
            RatingName = name,
            RatingWeight = ratingWeight,
            RatingCategory = category
        };

        _db.Add(rt);
        await _db.SaveChangesAsync(ct);
        return rt.RatingTypeId;
    }

    public async Task UpdateAllAsync(int ratingTypeId, RatingTypeDto updatedRating, CancellationToken ct = default)
    {
        var rt = await _db.Set<RatingType>()
            .FirstOrDefaultAsync(x => x.RatingTypeId == ratingTypeId, ct);

        if (rt is null) return;

        _mapper.Map(updatedRating, rt); // RatingTypeId is ignored in your profile
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateWeightAsync(int ratingTypeId, decimal newRatingWeight, CancellationToken ct = default)
    {
        var rt = await _db.Set<RatingType>()
            .FirstOrDefaultAsync(x => x.RatingTypeId == ratingTypeId, ct);

        if (rt is null) return;

        rt.RatingWeight = newRatingWeight;
        await _db.SaveChangesAsync(ct);
    }

    public async Task RenameAsync(int ratingTypeId, string newName, CancellationToken ct = default)
    {
        var rt = await _db.Set<RatingType>()
            .FirstOrDefaultAsync(x => x.RatingTypeId == ratingTypeId, ct);

        if (rt is null) return;

        rt.RatingName = newName;
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int ratingTypeId, CancellationToken ct = default)
    {
        // All-or-nothing
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        // 1) Delete person ratings that use this type
        var personRatings = await _db.Set<PersonRating>()
            .Where(r => r.RatingTypeId == ratingTypeId)
            .ToListAsync(ct);

        if (personRatings.Count > 0)
        {
            _db.Set<PersonRating>().RemoveRange(personRatings);
        }

        // 2) Delete relationship ratings that use this type
        var relationshipRatings = await _db.Set<RelationshipRating>()
            .Where(r => r.RatingTypeId == ratingTypeId)
            .ToListAsync(ct);

        if (relationshipRatings.Count > 0)
        {
            _db.Set<RelationshipRating>().RemoveRange(relationshipRatings);
        }

        // 3) Delete the rating type itself
        var rt = await _db.Set<RatingType>()
            .FirstOrDefaultAsync(x => x.RatingTypeId == ratingTypeId, ct);

        if (rt is not null)
        {
            _db.Remove(rt);
        }

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}

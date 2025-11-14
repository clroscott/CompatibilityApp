using CompatibilityApp.Domain.DayFiance.People;
using CompatibilityApp.Infrastructure.Data;
using CompatibilityApp.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection;

using AutoMapper;


namespace CompatibilityApp.Infrastructure.DayFiance.People;

/// <summary>
/// DB-first implementation. Uses Relationship + RelationshipSeason for partner data.
/// </summary>
public sealed class PersonService : IPersonService
{
    private readonly CompatibiltyDBContext _db; 
    private readonly IMapper _mapper;

    public PersonService(CompatibiltyDBContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    // --- helpers -------------------------------------------------------------
    public async Task<IReadOnlyList<PersonSeasonDto>> GetPeopleSeasonAsync(short? season = null, CancellationToken ct = default)
    {
        var q = _db.Set<PersonSeason>().AsNoTracking();

        if (season is not null)
            q = q.Where(ps => ps.Season == season.Value);

        // 1) get roster (without partner)
        var roster = await q
            .Join(_db.Set<Person>().AsNoTracking(),
                  ps => ps.PersonId,
                  p => p.PersonId,
                  (ps, p) => new PersonSeasonDto
                  {
                      PersonId = ps.PersonId,
                      Season = ps.Season,
                      FirstName = p.FirstName,
                      LastName = p.LastName,
                      DateOfBirth = p.DateOfBirth,
                      HomeCity = p.HomeCity,
                      HomeCountry = p.HomeCountry,
                      PersonType = p.PersonType,
                      Gender = p.Gender,
                      PartnerPersonId = null
                  })
            .ToListAsync(ct);

        if (roster.Count == 0)
            return roster;

        // 2) collect seasons present (handles season == null case)
        var seasons = roster.Select(r => r.Season).Distinct().ToList();
        var personIds = roster.Select(r => r.PersonId).Distinct().ToList();

        // 3) load Relationship + RelationshipSeason rows for these seasons and people
        var rels = await (
            from rs in _db.Set<RelationshipSeason>().AsNoTracking()
            join r in _db.Set<Relationship>().AsNoTracking() on rs.RelationshipId equals r.RelationshipId
            where seasons.Contains(rs.Season) &&
                  (personIds.Contains(r.PersonId1) || personIds.Contains(r.PersonId2))
            select new
            {
                rs.Season,
                r.PersonId1,
                r.PersonId2
            }
        ).ToListAsync(ct);

        if (rels.Count == 0)
            return roster;

        // 4) build fast lookup map keyed by (personId, season) -> partnerId
        var map = new Dictionary<(int personId, short season), int>();

        foreach (var r in rels)
        {
            // note: ensure symmetry: person1 -> person2 and person2 -> person1 for the same season
            map[(r.PersonId1, r.Season)] = r.PersonId2;
            map[(r.PersonId2, r.Season)] = r.PersonId1;
        }

        // 5) apply map to roster
        foreach (var item in roster)
        {
            if (map.TryGetValue((item.PersonId, item.Season), out var partner))
                item.PartnerPersonId = partner;
        }

        return roster;
    }

    public async Task<IReadOnlyList<PersonWithRatingsDto>> GetPeopleWithRatingsAsync(short? season = null, CancellationToken ct = default)
    {
        IQueryable<PersonSeason> personSeasonQuery = _db.Set<PersonSeason>().AsNoTracking();

        if (season is not null)
        {
            personSeasonQuery = personSeasonQuery.Where(ps => ps.Season == season.Value);
        }

        var roster = await (
            from ps in personSeasonQuery
            join p in _db.Set<Person>().AsNoTracking() on ps.PersonId equals p.PersonId
            select new
            {
                ps.PersonId,
                ps.Season,
                p.FirstName,
                p.LastName,
                p.DateOfBirth,
                p.HomeCity,
                p.HomeCountry,
                p.PersonType,
                p.Gender,
                p.ImagePath
            }
        ).ToListAsync(ct);

        if (roster.Count == 0)
            return Array.Empty<PersonWithRatingsDto>();

        var personIds = roster.Select(r => r.PersonId).Distinct().ToList();

        IQueryable<PersonRating> ratingsQuery = _db.Set<PersonRating>().AsNoTracking()
            .Where(r => personIds.Contains(r.PersonId));

        if (season is not null)
        {
            ratingsQuery = ratingsQuery.Where(r => r.Season == season.Value);
        }

        var ratings = await ratingsQuery.ToListAsync(ct);

        var mapByPerson = ratings
            .GroupBy(r => r.PersonId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyDictionary<int, decimal?>)g.ToDictionary(x => x.RatingTypeId, x => (decimal?)x.RatingValue)
            );

        var result = roster
            .GroupBy(r => r.PersonId)
            .Select(group =>
            {
                var first = group.First();

                return new PersonWithRatingsDto
                {
                    PersonId = first.PersonId,
                    FirstName = first.FirstName,
                    LastName = first.LastName,
                    DateOfBirth = first.DateOfBirth,
                    Season = season ?? first.Season,
                    HomeCity = first.HomeCity,
                    HomeCountry = first.HomeCountry,
                    PersonType = first.PersonType,
                    Gender = first.Gender,
                    ImagePath = first.ImagePath,

                    RatingsByTypeId = mapByPerson.TryGetValue(first.PersonId, out var dict) ? dict : new Dictionary<int, decimal?>()
                };
            })
            .ToList();

        return result;
    }

    // --- people CRUD ---------------------------------------------------------
    public async Task<int> AddPersonAsync(PersonDto dto, CancellationToken ct = default)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        var p = _mapper.Map<Person>(dto);

        _db.Add(p);
        await _db.SaveChangesAsync(ct);

        return p.PersonId;
    }

    public async Task UpdatePersonAsync(int personId, PersonDto dto, CancellationToken ct = default)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        var p = await _db.Set<Person>().FindAsync(new object?[] { personId }, ct)
                ?? throw new KeyNotFoundException($"Person {personId} not found");

        _mapper.Map(dto, p); // maps onto existing entity (PersonId ignored in profile)

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeletePersonAsync(int personId, CancellationToken ct = default)
    {
        var p = await _db.Set<Person>().FindAsync(new object?[] { personId }, ct);
        if (p is null) return;
        _db.Remove(p);
        await _db.SaveChangesAsync(ct);
    }

    // --- additional queries --------------------------------------------------
    public async Task<IReadOnlyList<PersonDto>> GetAllPeopleAsync(CancellationToken ct = default)
    {
        var peopleEntities = await _db.Set<Person>()
            .AsNoTracking()
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync(ct);

        return _mapper.Map<List<PersonDto>>(peopleEntities);
    }

    // --- season CRUD ---------------------------------------------------------
    public async Task UpsertPersonSeasonAsync(int personId, short season, CancellationToken ct = default)
    {
        var existing = await _db.Set<PersonSeason>().FindAsync(new object?[] { personId, season }, ct);
        if (existing is null)
        {
            var ps = new PersonSeason { PersonId = personId, Season = season };
            _db.Set<PersonSeason>().Add(ps);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task AddPersonToSeasonAsync(int personId, short season, CancellationToken ct = default)
    {
        // Ensure person exists
        var exists = await _db.Set<Person>().AsNoTracking().AnyAsync(p => p.PersonId == personId, ct);
        if (!exists)
            throw new KeyNotFoundException($"Person {personId} not found");

        // Delegate to UpsertPersonSeasonAsync to reuse logic / avoid duplicates
        await UpsertPersonSeasonAsync(personId, season, ct);
    }

    public async Task RemovePersonFromSeasonAsync(int personId, short season, CancellationToken ct = default)
    {
        var ps = await _db.Set<PersonSeason>().FindAsync(new object?[] { personId, season }, ct);
        if (ps is null) return;

        // Remove any ratings for this person + season first (cleanup)
        var ratings = await _db.Set<PersonRating>()
            .Where(r => r.PersonId == personId && r.Season == season)
            .ToListAsync(ct);

        if (ratings.Count > 0)
            _db.Set<PersonRating>().RemoveRange(ratings);

        _db.Set<PersonSeason>().Remove(ps);

        // --- additionally remove any relationship entries that reference this person for that season ---
        // 1) find RelationshipSeason rows for this season where the relationship involves this person
        var relSeasonsToRemove = await (
            from rs in _db.Set<RelationshipSeason>()
            join r in _db.Set<Relationship>() on rs.RelationshipId equals r.RelationshipId
            where rs.Season == season && (r.PersonId1 == personId || r.PersonId2 == personId)
            select rs
        ).ToListAsync(ct);

        if (relSeasonsToRemove.Count > 0)
        {
            var relIds = relSeasonsToRemove.Select(x => x.RelationshipId).Distinct().ToList();

            // remove any relationship ratings tied to these relationship+season rows (cleanup)
            var relRatingsToRemove = await _db.Set<RelationshipRating>()
                .Where(rr => relIds.Contains(rr.RelationshipId) && rr.Season == season)
                .ToListAsync(ct);

            if (relRatingsToRemove.Count > 0)
                _db.Set<RelationshipRating>().RemoveRange(relRatingsToRemove);

            // remove the RelationshipSeason rows for this season
            _db.Set<RelationshipSeason>().RemoveRange(relSeasonsToRemove);
            await _db.SaveChangesAsync(ct);

            // remove relationships that no longer have any seasons
            var orphanRels = await _db.Set<Relationship>()
                .Where(r => relIds.Contains(r.RelationshipId))
                .Where(r => !_db.Set<RelationshipSeason>().Any(rs => rs.RelationshipId == r.RelationshipId))
                .ToListAsync(ct);

            if (orphanRels.Count > 0)
            {
                _db.Set<Relationship>().RemoveRange(orphanRels);
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    // --- relationship management using Relationship/RelationshipSeason ----------
    public async Task SetPersonRelationshipAsync(int personId, int? partnerPersonId, short season, CancellationToken ct = default)
    {
        // validate people
        var personExists = await _db.Set<Person>().AsNoTracking().AnyAsync(p => p.PersonId == personId, ct);
        if (!personExists) throw new KeyNotFoundException($"Person {personId} not found");

        if (partnerPersonId.HasValue)
        {
            var partnerExists = await _db.Set<Person>().AsNoTracking().AnyAsync(p => p.PersonId == partnerPersonId.Value, ct);
            if (!partnerExists) throw new KeyNotFoundException($"Partner person {partnerPersonId.Value} not found");
        }

        if (!partnerPersonId.HasValue)
        {
            // remove any RelationshipSeason rows for the given person+season
            var relSeasonsToRemove = await (
                from rs in _db.Set<RelationshipSeason>()
                join r in _db.Set<Relationship>() on rs.RelationshipId equals r.RelationshipId
                where rs.Season == season && (r.PersonId1 == personId || r.PersonId2 == personId)
                select rs
            ).ToListAsync(ct);

            if (relSeasonsToRemove.Count > 0)
            {
                var relIds = relSeasonsToRemove.Select(x => x.RelationshipId).Distinct().ToList();

                _db.Set<RelationshipSeason>().RemoveRange(relSeasonsToRemove);
                await _db.SaveChangesAsync(ct);

                // remove relationships that no longer have any seasons
                var orphanRels = await _db.Set<Relationship>()
                    .Where(r => relIds.Contains(r.RelationshipId))
                    .Where(r => !_db.Set<RelationshipSeason>().Any(rs => rs.RelationshipId == r.RelationshipId))
                    .ToListAsync(ct);

                if (orphanRels.Count > 0)
                {
                    _db.Set<Relationship>().RemoveRange(orphanRels);
                    await _db.SaveChangesAsync(ct);
                }
            }

            return;
        }

        // partner provided: find or create relationship for the pair
        var partnerId = partnerPersonId.Value;

        var rel = await _db.Set<Relationship>()
            .SingleOrDefaultAsync(r =>
                (r.PersonId1 == personId && r.PersonId2 == partnerId) ||
                (r.PersonId1 == partnerId && r.PersonId2 == personId), ct);

        if (rel is null)
        {
            rel = new Relationship
            {
                PersonId1 = personId,
                PersonId2 = partnerId,
                RelationshipType = "Fiance",
                ImagePath = ""
            };
            _db.Set<Relationship>().Add(rel);
            await _db.SaveChangesAsync(ct); // get RealtionshipId
        }

        // ensure RelationshipSeason exists
        var existsRs = await _db.Set<RelationshipSeason>().FindAsync(new object?[] { season, rel.RelationshipId }, ct);
        if (existsRs is null)
        {
            var rs = new RelationshipSeason { Season = season, RelationshipId = rel.RelationshipId };
            _db.Set<RelationshipSeason>().Add(rs);
            await _db.SaveChangesAsync(ct);
        }
    }

    // --- ratings on person ---------------------------------------------------
    public async Task UpsertPersonRatingAsync(int personId, int ratingTypeId, decimal? rating, short? season = null, CancellationToken ct = default)
    {
        if (season is null)
            throw new ArgumentNullException(nameof(season), "Season is required when setting a rating.");

        if (rating is null)
            throw new ArgumentNullException(nameof(rating), "Rating value is required when upserting a rating.");

        short s = season.Value;

        var personSeason = await _db.Set<PersonSeason>().FindAsync(new object?[] { personId, s }, ct);
        if (personSeason is null)
        {
            personSeason = new PersonSeason { PersonId = personId, Season = s };
            _db.Set<PersonSeason>().Add(personSeason);
        }

        var existing = await _db.Set<PersonRating>().SingleOrDefaultAsync(r =>
            r.PersonId == personId &&
            r.Season == s &&
            r.RatingTypeId == ratingTypeId, ct);

        if (existing is null)
        {
            var pr = new PersonRating
            {
                PersonId = personId,
                Season = s,
                RatingTypeId = ratingTypeId,
                RatingValue = rating.Value
            };

            _db.Set<PersonRating>().Add(pr);
        }
        else
        {
            existing.RatingValue = rating.Value;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task RemovePersonRatingAsync(int personId, int ratingTypeId, short? season = null, CancellationToken ct = default)
    {
        var q = _db.Set<PersonRating>().Where(r =>
            r.PersonId == personId &&
            r.RatingTypeId == ratingTypeId);

        if (season is not null)
            q = q.Where(r => r.Season == season.Value);

        var rows = await q.ToListAsync(ct);
        if (rows.Count == 0) return;

        _db.RemoveRange(rows);
        await _db.SaveChangesAsync(ct);
    }
}

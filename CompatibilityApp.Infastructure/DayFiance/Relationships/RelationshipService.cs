using AutoMapper;
using CompatibilityApp.Domain.DayFiance.Relationships;
using CompatibilityApp.Infrastructure.Data;
using CompatibilityApp.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace CompatibilityApp.Infrastructure.DayFiance.Relationships
{
    /// <summary>
    /// DB-first implementation for relationship ratings.
    /// Uses Relationship + RelationshipSeason + RelationshipRating.
    /// </summary>
    public sealed class RelationshipService : IRelationshipService
    {
        private readonly CompatibiltyDBContext _db;
        private readonly IMapper _mapper; // reserved for CRUD mapping if needed later

        public RelationshipService(CompatibiltyDBContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        // local helper "row shape" for the roster query
        private sealed class RelationshipRosterRow
        {
            public short Season { get; init; }
            public int RelationshipId { get; init; }
            public int PersonId1 { get; init; }
            public int PersonId2 { get; init; }
            public string Person1FirstName { get; init; } = "";
            public string Person1LastName { get; init; } = "";
            public string Person2FirstName { get; init; } = "";
            public string Person2LastName { get; init; } = "";
            public string RelationshipType { get; init; } = "";
            public string ImagePath { get; init; } = "";
            public string Person1ImagePath { get; init; } = "";
            public string Person2ImagePath { get; init; } = "";
        }

        private static RelationshipWithRatingsDto ToDto(
            RelationshipRosterRow row,
            short? requestedSeason,
            IReadOnlyDictionary<int, decimal?> ratings)
        {
            return new RelationshipWithRatingsDto
            {
                RelationshipId = row.RelationshipId,
                Season = requestedSeason ?? row.Season,
                PersonId1 = row.PersonId1,
                PersonId2 = row.PersonId2,
                Person1FirstName = row.Person1FirstName,
                Person1LastName = row.Person1LastName,
                Person2FirstName = row.Person2FirstName,
                Person2LastName = row.Person2LastName,
                RelationshipType = row.RelationshipType,
                RatingsByTypeId = ratings,
                ImagePath = row.ImagePath,
                Person1ImagePath = row.Person1ImagePath,
                Person2ImagePath = row.Person2ImagePath
            };
        }

        // ---------------------------------------------------------------------
        // Grid query: relationships + ratings for an optional season
        // ---------------------------------------------------------------------
        public async Task<IReadOnlyList<RelationshipWithRatingsDto>> GetRelationshipsWithRatingsAsync(
            short? season = null,
            CancellationToken ct = default)
        {
            IQueryable<RelationshipSeason> rsQuery =
                _db.Set<RelationshipSeason>().AsNoTracking();

            if (season is not null)
            {
                rsQuery = rsQuery.Where(rs => rs.Season == season.Value);
            }

            // 1) base roster: relationship + season + people names
            var roster = await (
                from rs in rsQuery
                join rel in _db.Set<Relationship>().AsNoTracking()
                    on rs.RelationshipId equals rel.RelationshipId
                join p1 in _db.Set<Person>().AsNoTracking()
                    on rel.PersonId1 equals p1.PersonId
                join p2 in _db.Set<Person>().AsNoTracking()
                    on rel.PersonId2 equals p2.PersonId
                select new RelationshipRosterRow
                {
                    Season = rs.Season,
                    RelationshipId = rel.RelationshipId,
                    PersonId1 = rel.PersonId1,
                    PersonId2 = rel.PersonId2,
                    RelationshipType = rel.RelationshipType,
                    Person1FirstName = p1.FirstName,
                    Person1LastName = p1.LastName,
                    Person2FirstName = p2.FirstName,
                    Person2LastName = p2.LastName,
                    ImagePath = rel.ImagePath,
                    Person1ImagePath = p1.ImagePath,
                    Person2ImagePath = p2.ImagePath

                }
            ).ToListAsync(ct);

            if (roster.Count == 0)
                return Array.Empty<RelationshipWithRatingsDto>();

            var relationshipIds = roster
                .Select(r => r.RelationshipId)
                .Distinct()
                .ToList();

            // 2) pull ratings for those relationships (optionally by season)
            IQueryable<RelationshipRating> ratingsQuery =
                _db.Set<RelationshipRating>().AsNoTracking()
                    .Where(rr => relationshipIds.Contains(rr.RelationshipId));

            if (season is not null)
            {
                ratingsQuery = ratingsQuery.Where(rr => rr.Season == season.Value);
            }

            var ratings = await ratingsQuery.ToListAsync(ct);

            // 3) build map: relationshipId -> (ratingTypeId -> rating)
            var mapByRelationship = ratings
                .GroupBy(r => r.RelationshipId)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyDictionary<int, decimal?>)g.ToDictionary(
                        x => x.RatingTypeId,
                        x => (decimal?)x.RatingValue
                    )
                );

            // 4) project to DTOs (one per relationship)
            var result = roster
                .GroupBy(r => r.RelationshipId)
                .Select(g =>
                {
                    var first = g.First();

                    var ratingsDict = mapByRelationship.TryGetValue(first.RelationshipId, out var dict)
                        ? dict
                        : new Dictionary<int, decimal?>();

                    return ToDto(first, season, ratingsDict);
                })
                .ToList();

            return result;
        }

        // ---------------------------------------------------------------------
        // Ratings on relationships (RelationshipRating)
        // ---------------------------------------------------------------------
        public async Task UpsertRelationshipRatingAsync(
            int relationshipId,
            int ratingTypeId,
            decimal? rating,
            short? season = null,
            CancellationToken ct = default)
        {
            if (season is null)
                throw new ArgumentNullException(nameof(season), "Season is required when setting a relationship rating.");

            if (rating is null)
                throw new ArgumentNullException(nameof(rating), "Rating value is required when upserting a relationship rating.");

            // Optional safety: ensure the relationship exists
            var exists = await _db.Set<Relationship>()
                .AsNoTracking()
                .AnyAsync(r => r.RelationshipId == relationshipId, ct);

            if (!exists)
                throw new KeyNotFoundException($"Relationship {relationshipId} not found.");

            short s = season.Value;

            // Ensure RelationshipSeason exists for this relationship + season
            var relSeason = await _db.Set<RelationshipSeason>()
                .FindAsync(new object?[] { s, relationshipId }, ct);

            if (relSeason is null)
            {
                relSeason = new RelationshipSeason
                {
                    Season = s,
                    RelationshipId = relationshipId
                };
                _db.Set<RelationshipSeason>().Add(relSeason);
            }

            // Upsert RelationshipRating
            var existing = await _db.Set<RelationshipRating>()
                .SingleOrDefaultAsync(rr =>
                    rr.RelationshipId == relationshipId &&
                    rr.Season == s &&
                    rr.RatingTypeId == ratingTypeId,
                    ct);

            if (existing is null)
            {
                var rr = new RelationshipRating
                {
                    RelationshipId = relationshipId,
                    Season = s,
                    RatingTypeId = ratingTypeId,
                    RatingValue = rating.Value
                };

                _db.Set<RelationshipRating>().Add(rr);
            }
            else
            {
                existing.RatingValue = rating.Value;
            }

            await _db.SaveChangesAsync(ct);
        }

        public async Task RemoveRelationshipRatingAsync(
            int relationshipId,
            int ratingTypeId,
            short? season = null,
            CancellationToken ct = default)
        {
            var q = _db.Set<RelationshipRating>().Where(rr =>
                rr.RelationshipId == relationshipId &&
                rr.RatingTypeId == ratingTypeId);

            if (season is not null)
            {
                q = q.Where(rr => rr.Season == season.Value);
            }

            var rows = await q.ToListAsync(ct);
            if (rows.Count == 0) return;

            _db.RemoveRange(rows);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateRelationshipImagePathAsync(int relationshipId, string imagePath, CancellationToken ct = default)
        {
            var rel = await _db.Set<Relationship>()
                .FindAsync(new object?[] { relationshipId }, ct);

            if (rel is null)
                throw new KeyNotFoundException($"Relationship {relationshipId} not found");

            rel.ImagePath = imagePath;
            await _db.SaveChangesAsync(ct);
        }

    }
}

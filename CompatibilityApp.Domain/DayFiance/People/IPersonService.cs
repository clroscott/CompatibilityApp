namespace CompatibilityApp.Domain.DayFiance.People;

public interface IPersonService
{
    // Roster queries
    Task<IReadOnlyList<PersonSeasonDto>> GetPeopleSeasonAsync(short? season = null, CancellationToken ct = default);
    Task<IReadOnlyList<PersonWithRatingsDto>> GetPeopleWithRatingsAsync(short? season = null, CancellationToken ct = default);

    // All people (for "add existing person" dropdown & management)
    Task<IReadOnlyList<PersonDto>> GetAllPeopleAsync(CancellationToken ct = default);

    // Season membership
    Task AddPersonToSeasonAsync(int personId, short season, CancellationToken ct = default);
    Task RemovePersonFromSeasonAsync(int personId, short season, CancellationToken ct = default);
    Task UpsertPersonSeasonAsync(int personId, short season, CancellationToken ct = default);

    // Relationship management (Relationship + RelationshipSeason)
    Task SetPersonRelationshipAsync(int personId, int? partnerPersonId, short season, CancellationToken ct = default);

    // People CRUD
    Task<int> AddPersonAsync(PersonDto dto, CancellationToken ct = default);
    Task UpdatePersonAsync(int personId, PersonDto dto, CancellationToken ct = default);
    Task DeletePersonAsync(int personId, CancellationToken ct = default);

    // Ratings on a person (keyed by RatingTypeId; season optional)
    Task UpsertPersonRatingAsync(int personId, int ratingTypeId, decimal? rating, short? season = null, CancellationToken ct = default);
    Task RemovePersonRatingAsync(int personId, int ratingTypeId, short? season = null, CancellationToken ct = default);
}
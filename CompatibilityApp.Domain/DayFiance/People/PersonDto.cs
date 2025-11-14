namespace CompatibilityApp.Domain.DayFiance.People;

/// <summary>
/// Base class for person-related DTOs.
/// Contains shared identity and birth information.
/// </summary>
public class PersonDto
{
    public int PersonId { get; init; }
    public string FirstName { get; init; } = "";
    public string LastName { get; init; } = "";

    public DateTime DateOfBirth { get; init; }
    public string HomeCity { get; init; } = "";
    public string HomeCountry { get; init; } = "";
    public string PersonType { get; init; } = "";
    public string Gender { get; init; } = "";

    public string ImagePath { get; init; } = "";

    public int Age
    {
        get
        {
            return DateTime.Now.Year - DateOfBirth.Year - (DateTime.Now.Date < DateOfBirth.AddYears(DateTime.Now.Year - DateOfBirth.Year) ? 1 : 0);
        }
    }

    public string FullName => $"{FirstName} {LastName}".Trim();

}

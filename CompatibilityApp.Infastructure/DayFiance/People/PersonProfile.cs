using AutoMapper;
using CompatibilityApp.Domain.DayFiance.People;
using CompatibilityApp.Infrastructure.Entities;

namespace CompatibilityApp.Infrastructure.DayFiance.People;

public sealed class PersonProfile : Profile
{
    public PersonProfile()
    {
        // Entity -> DTO
        CreateMap<Person, PersonDto>();

        // DTO -> Entity
        CreateMap<PersonDto, Person>()
            .ForMember(dest => dest.PersonId, opt => opt.Ignore());
        // PK comes from DB, don’t overwrite it on update


        //CreateMap<PersonSeasonDto, PersonSeason>();
        //CreateMap<PersonSeason, PersonSeasonDto>()
        //    .ForMember(dest => dest.PersonId, opt => opt.Ignore());


        //CreateMap<PersonWithRatingsDto, PersonRating>();
        //CreateMap<PersonRating, PersonWithRatingsDto>()
        //    .ForMember(dest => dest.PersonId, opt => opt.Ignore());
    }
}

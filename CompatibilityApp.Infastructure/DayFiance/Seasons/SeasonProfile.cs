using AutoMapper;
using CompatibilityApp.Domain.DayFiance.Seasons;
using CompatibilityApp.Infrastructure.Entities;

namespace CompatibilityApp.Infrastructure.DayFiance.Seasons
{
    public sealed class SeasonProfile : Profile
    {
        public SeasonProfile()
        {
            // Entity <-> DTO, by convention (SeasonNum, AirDate, FilmingDate)
            CreateMap<Season, SeasonDto>()
                .ReverseMap();
        }
    }
}

using AutoMapper;
using CompatibilityApp.Domain.DayFiance.Ratings;
using CompatibilityApp.Infrastructure.Entities;

namespace CompatibilityApp.Infrastructure.DayFiance.Ratings;

public sealed class RatingTypeProfile : Profile
{
    public RatingTypeProfile()
    {
        // Entity -> DTO
        CreateMap<RatingType, RatingTypeDto>();


        // DTO -> Entity
        CreateMap<RatingTypeDto, RatingType>()
            .ForMember(dest => dest.RatingTypeId, opt => opt.Ignore());            // PK handled by DB
    }
}

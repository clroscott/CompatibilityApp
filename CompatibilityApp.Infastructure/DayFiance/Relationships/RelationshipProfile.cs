using AutoMapper;
using CompatibilityApp.Domain.DayFiance.Relationships;
using CompatibilityApp.Infrastructure.Entities;

namespace CompatibilityApp.Infrastructure.DayFiance.Relationships;

public sealed class RelationshipProfile : Profile
{
    public RelationshipProfile()
    {
        // Entity -> DTO
        CreateMap<Relationship, RelationshipDto>();

        // DTO -> Entity
        CreateMap<RelationshipDto, Relationship>()
            .ForMember(dest => dest.RelationshipId, opt => opt.Ignore()); // PK from DB
    }
}

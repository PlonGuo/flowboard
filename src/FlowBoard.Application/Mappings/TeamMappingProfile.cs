using AutoMapper;
using FlowBoard.Application.DTOs;
using FlowBoard.Core.Entities;

namespace FlowBoard.Application.Mappings;

public class TeamMappingProfile : Profile
{
    public TeamMappingProfile()
    {
        CreateMap<Team, TeamDto>()
            .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.Members.Count));

        CreateMap<TeamMember, TeamMemberDto>();
    }
}

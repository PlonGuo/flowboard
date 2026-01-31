using AutoMapper;
using FlowBoard.Application.DTOs;
using FlowBoard.Core.Entities;

namespace FlowBoard.Application.Mappings;

public class BoardMappingProfile : Profile
{
    public BoardMappingProfile()
    {
        CreateMap<Board, BoardDto>()
            .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Team.Name));

        CreateMap<Board, BoardDetailDto>()
            .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Team.Name));

        CreateMap<Board, BoardSummaryDto>()
            .ForMember(dest => dest.TaskCount, opt => opt.MapFrom(src =>
                src.Columns.Sum(c => c.Tasks.Count)))
            .ForMember(dest => dest.ColumnCount, opt => opt.MapFrom(src => src.Columns.Count));
    }
}

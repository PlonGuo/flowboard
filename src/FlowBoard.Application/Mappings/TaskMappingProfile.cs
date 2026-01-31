using AutoMapper;
using FlowBoard.Application.DTOs;
using FlowBoard.Core.Entities;

namespace FlowBoard.Application.Mappings;

public class TaskMappingProfile : Profile
{
    public TaskMappingProfile()
    {
        CreateMap<TaskItem, TaskItemDto>()
            .ForMember(dest => dest.ColumnName, opt => opt.MapFrom(src => src.Column.Name))
            .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Comments.Count));

        CreateMap<TaskItem, TaskItemDetailDto>()
            .ForMember(dest => dest.ColumnName, opt => opt.MapFrom(src => src.Column.Name))
            .ForMember(dest => dest.BoardId, opt => opt.MapFrom(src => src.Column.BoardId))
            .ForMember(dest => dest.BoardName, opt => opt.MapFrom(src => src.Column.Board.Name));

        CreateMap<TaskItem, TaskItemSummaryDto>();
    }
}

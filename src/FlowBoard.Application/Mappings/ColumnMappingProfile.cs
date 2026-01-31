using AutoMapper;
using FlowBoard.Application.DTOs;
using FlowBoard.Core.Entities;

namespace FlowBoard.Application.Mappings;

public class ColumnMappingProfile : Profile
{
    public ColumnMappingProfile()
    {
        CreateMap<Column, ColumnDto>();

        CreateMap<Column, ColumnSummaryDto>()
            .ForMember(dest => dest.TaskCount, opt => opt.MapFrom(src => src.Tasks.Count));
    }
}

using AutoMapper;
using FlowBoard.Application.DTOs;
using FlowBoard.Core.Entities;

namespace FlowBoard.Application.Mappings;

public class CanvasMappingProfile : Profile
{
    public CanvasMappingProfile()
    {
        CreateMap<Canvas, CanvasDto>();

        CreateMap<Canvas, CanvasDetailDto>();

        CreateMap<CanvasData, CanvasDataDto>();
    }
}

using AutoMapper;
using FlowBoard.Application.DTOs;
using FlowBoard.Core.Entities;

namespace FlowBoard.Application.Mappings;

public class CommentMappingProfile : Profile
{
    public CommentMappingProfile()
    {
        CreateMap<Comment, CommentDto>();
    }
}

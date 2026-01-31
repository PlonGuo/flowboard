using AutoMapper;
using FlowBoard.Application.DTOs;
using FlowBoard.Core.Entities;

namespace FlowBoard.Application.Mappings;

public class NotificationMappingProfile : Profile
{
    public NotificationMappingProfile()
    {
        CreateMap<Notification, NotificationDto>();
    }
}

using AutoMapper;
using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using FlowBoard.Application.Queries.Notifications;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Notifications;

/// <summary>
/// Handler for getting notifications for a user.
/// </summary>
public class GetNotificationsHandler : IRequestHandler<GetNotificationsQuery, Result<IEnumerable<NotificationDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetNotificationsHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<NotificationDto>>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        // Get notifications for the user
        var notifications = await _unitOfWork.Notifications
            .FindAsync(n => n.UserId == request.UserId);

        // Filter by read status if requested
        if (!request.IncludeRead)
        {
            notifications = notifications.Where(n => !n.IsRead);
        }

        // Order by unread first, then by newest
        var orderedNotifications = notifications
            .OrderBy(n => n.IsRead)
            .ThenByDescending(n => n.CreatedAt)
            .Skip(request.Skip)
            .Take(request.Take)
            .ToList();

        var dtos = _mapper.Map<IEnumerable<NotificationDto>>(orderedNotifications);
        return Result.Success(dtos);
    }
}

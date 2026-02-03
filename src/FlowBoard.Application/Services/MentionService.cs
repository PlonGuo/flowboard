using System.Text.RegularExpressions;
using AutoMapper;
using FlowBoard.Application.DTOs;
using FlowBoard.Core.Entities;
using FlowBoard.Core.Enums;
using FlowBoard.Core.Interfaces;

namespace FlowBoard.Application.Services;

/// <summary>
/// Implementation of the mention service for parsing @mentions and creating notifications.
/// </summary>
public partial class MentionService : IMentionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserNotificationService _notificationService;
    private readonly IMapper _mapper;

    public MentionService(
        IUnitOfWork unitOfWork,
        IUserNotificationService notificationService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _mapper = mapper;
    }

    // Regex pattern to match @mentions (captures text after @ until whitespace or end)
    // Matches: @John, @John Doe, @John-Doe, etc.
    [GeneratedRegex(@"@([A-Za-z][A-Za-z\s\-']*[A-Za-z]|[A-Za-z])", RegexOptions.Compiled)]
    private static partial Regex MentionPattern();

    public IEnumerable<int> ParseMentions(string content, IEnumerable<UserSummaryDto> boardMembers)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return Enumerable.Empty<int>();
        }

        var membersList = boardMembers.ToList();
        if (membersList.Count == 0)
        {
            return Enumerable.Empty<int>();
        }

        var mentionedUserIds = new HashSet<int>();
        var matches = MentionPattern().Matches(content);

        foreach (Match match in matches)
        {
            var mentionText = match.Groups[1].Value.Trim();

            // Try to find a board member whose name matches the mention
            // Match against full name (case-insensitive)
            var matchedMember = membersList.FirstOrDefault(m =>
                m.FullName.Equals(mentionText, StringComparison.OrdinalIgnoreCase));

            // If no exact match, try partial match (first name only)
            if (matchedMember == null)
            {
                matchedMember = membersList.FirstOrDefault(m =>
                {
                    var firstName = m.FullName.Split(' ').FirstOrDefault() ?? "";
                    return firstName.Equals(mentionText, StringComparison.OrdinalIgnoreCase);
                });
            }

            if (matchedMember != null)
            {
                mentionedUserIds.Add(matchedMember.Id);
            }
        }

        return mentionedUserIds;
    }

    public async Task CreateMentionNotificationsAsync(
        int taskId,
        string taskTitle,
        int boardId,
        int commentAuthorId,
        string commentAuthorName,
        IEnumerable<int> mentionedUserIds,
        CancellationToken cancellationToken = default)
    {
        var userIds = mentionedUserIds
            .Where(id => id != commentAuthorId) // Don't notify the author about their own mention
            .Distinct()
            .ToList();

        if (userIds.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;

        foreach (var userId in userIds)
        {
            // Create the notification entity
            var notification = new Notification
            {
                UserId = userId,
                Type = NotificationType.Mentioned,
                Title = $"{commentAuthorName} mentioned you",
                Message = $"You were mentioned in a comment on \"{taskTitle}\"",
                RelatedTaskId = taskId,
                RelatedBoardId = boardId,
                IsRead = false,
                CreatedAt = now
            };

            await _unitOfWork.Notifications.AddAsync(notification);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send real-time notifications
        // We need to reload the notifications to get the IDs
        var createdNotifications = await _unitOfWork.Notifications
            .FindAsync(n => userIds.Contains(n.UserId) &&
                           n.Type == NotificationType.Mentioned &&
                           n.RelatedTaskId == taskId &&
                           n.CreatedAt == now);

        foreach (var notification in createdNotifications)
        {
            var dto = _mapper.Map<NotificationDto>(notification);
            await _notificationService.SendNotificationAsync(notification.UserId, dto);
        }
    }
}

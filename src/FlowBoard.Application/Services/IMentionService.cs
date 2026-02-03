using FlowBoard.Application.DTOs;

namespace FlowBoard.Application.Services;

/// <summary>
/// Service for parsing and processing @mentions in comments.
/// </summary>
public interface IMentionService
{
    /// <summary>
    /// Parse @mentions from content and return matching user IDs.
    /// </summary>
    /// <param name="content">The comment content to parse</param>
    /// <param name="boardMembers">List of board members to match against</param>
    /// <returns>List of user IDs that were mentioned</returns>
    IEnumerable<int> ParseMentions(string content, IEnumerable<UserSummaryDto> boardMembers);

    /// <summary>
    /// Create notifications for mentioned users and send real-time updates.
    /// </summary>
    /// <param name="taskId">The task ID where the comment was added</param>
    /// <param name="taskTitle">The task title for notification message</param>
    /// <param name="boardId">The board ID for notification reference</param>
    /// <param name="commentAuthorId">The user who wrote the comment</param>
    /// <param name="commentAuthorName">The name of the comment author</param>
    /// <param name="mentionedUserIds">List of user IDs that were mentioned</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CreateMentionNotificationsAsync(
        int taskId,
        string taskTitle,
        int boardId,
        int commentAuthorId,
        string commentAuthorName,
        IEnumerable<int> mentionedUserIds,
        CancellationToken cancellationToken = default);
}

using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using MediatR;

namespace FlowBoard.Application.Commands.Comments;

/// <summary>
/// Command to create a new comment on a task.
/// </summary>
public record CreateCommentCommand(
    string Content,
    int TaskId,
    int UserId,
    string? ConnectionId = null
) : IRequest<Result<CommentDto>>;

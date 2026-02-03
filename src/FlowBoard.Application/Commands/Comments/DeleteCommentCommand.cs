using FlowBoard.Application.Common;
using MediatR;

namespace FlowBoard.Application.Commands.Comments;

/// <summary>
/// Command to delete a comment.
/// </summary>
public record DeleteCommentCommand(
    int CommentId,
    int UserId,
    string? ConnectionId = null
) : IRequest<Result<Unit>>;

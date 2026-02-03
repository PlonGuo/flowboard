using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using MediatR;

namespace FlowBoard.Application.Commands.Comments;

/// <summary>
/// Command to update an existing comment.
/// </summary>
public record UpdateCommentCommand(
    int CommentId,
    string Content,
    int UserId,
    string? ConnectionId = null
) : IRequest<Result<CommentDto>>;

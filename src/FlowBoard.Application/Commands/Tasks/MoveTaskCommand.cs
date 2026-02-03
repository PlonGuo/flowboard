using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using MediatR;

namespace FlowBoard.Application.Commands.Tasks;

/// <summary>
/// Command to move a task to a different column and/or position.
/// </summary>
public record MoveTaskCommand(
    int Id,
    int ToColumnId,
    int ToPosition,
    int UserId,
    byte[] RowVersion,
    string? ConnectionId = null
) : IRequest<Result<TaskItemDto>>;

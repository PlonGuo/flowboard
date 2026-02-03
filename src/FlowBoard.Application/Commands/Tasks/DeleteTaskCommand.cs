using FlowBoard.Application.Common;
using MediatR;

namespace FlowBoard.Application.Commands.Tasks;

/// <summary>
/// Command to delete a task.
/// </summary>
public record DeleteTaskCommand(
    int Id,
    int UserId,
    string? ConnectionId = null
) : IRequest<Result>;

using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using FlowBoard.Core.Enums;
using MediatR;

namespace FlowBoard.Application.Commands.Tasks;

/// <summary>
/// Command to update an existing task.
/// </summary>
public record UpdateTaskCommand(
    int Id,
    string Title,
    string? Description,
    TaskPriority Priority,
    int? AssigneeId,
    DateTime? DueDate,
    int UserId,
    byte[] RowVersion,
    string? ConnectionId = null
) : IRequest<Result<TaskItemDto>>;

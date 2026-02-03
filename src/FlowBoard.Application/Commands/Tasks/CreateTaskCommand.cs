using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using FlowBoard.Core.Enums;
using MediatR;

namespace FlowBoard.Application.Commands.Tasks;

/// <summary>
/// Command to create a new task in a column.
/// </summary>
public record CreateTaskCommand(
    string Title,
    string? Description,
    int ColumnId,
    TaskPriority Priority,
    int? AssigneeId,
    DateTime? DueDate,
    int UserId,
    string? ConnectionId = null
) : IRequest<Result<TaskItemDto>>;

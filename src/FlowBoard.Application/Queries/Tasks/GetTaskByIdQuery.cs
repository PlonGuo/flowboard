using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using MediatR;

namespace FlowBoard.Application.Queries.Tasks;

/// <summary>
/// Query to get a task by its ID.
/// </summary>
public record GetTaskByIdQuery(
    int Id,
    int UserId,
    bool IncludeComments = false
) : IRequest<Result<TaskItemDetailDto>>;

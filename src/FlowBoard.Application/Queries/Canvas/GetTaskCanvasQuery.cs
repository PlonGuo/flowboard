using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using MediatR;

namespace FlowBoard.Application.Queries.Canvas;

/// <summary>
/// Query to get canvas for a specific task.
/// Returns null if task has no canvas.
/// </summary>
public record GetTaskCanvasQuery(
    int TaskId,
    int UserId
) : IRequest<Result<CanvasDetailDto?>>;

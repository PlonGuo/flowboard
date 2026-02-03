using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using MediatR;

namespace FlowBoard.Application.Commands.Canvas;

/// <summary>
/// Command to create a canvas for a task.
/// </summary>
public record CreateTaskCanvasCommand(
    int TaskId,
    string Name,
    int UserId
) : IRequest<Result<CanvasDetailDto>>;

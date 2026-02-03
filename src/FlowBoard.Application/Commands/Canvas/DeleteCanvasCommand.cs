using FlowBoard.Application.Common;
using MediatR;

namespace FlowBoard.Application.Commands.Canvas;

/// <summary>
/// Command to delete a canvas.
/// </summary>
public record DeleteCanvasCommand(
    int CanvasId,
    int UserId
) : IRequest<Result>;

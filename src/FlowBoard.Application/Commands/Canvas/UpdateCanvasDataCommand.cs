using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using MediatR;

namespace FlowBoard.Application.Commands.Canvas;

/// <summary>
/// Command to update canvas data (elements, appState, files).
/// </summary>
public record UpdateCanvasDataCommand(
    int CanvasId,
    string Elements,
    string? AppState,
    string? Files,
    int UserId,
    string? ConnectionId = null
) : IRequest<Result<SaveCanvasDataResponse>>;

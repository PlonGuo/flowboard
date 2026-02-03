using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using MediatR;

namespace FlowBoard.Application.Queries.Canvas;

/// <summary>
/// Query to get canvas by ID with full data.
/// </summary>
public record GetCanvasByIdQuery(
    int CanvasId,
    int UserId
) : IRequest<Result<CanvasDetailDto>>;

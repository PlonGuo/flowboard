using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using MediatR;

namespace FlowBoard.Application.Commands.Boards;

/// <summary>
/// Command to update an existing board.
/// </summary>
public record UpdateBoardCommand(
    int Id,
    string Name,
    string? Description,
    int UserId
) : IRequest<Result<BoardDto>>;

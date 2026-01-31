using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using MediatR;

namespace FlowBoard.Application.Commands.Boards;

/// <summary>
/// Command to create a new board.
/// </summary>
public record CreateBoardCommand(
    string Name,
    string? Description,
    int TeamId,
    int UserId
) : IRequest<Result<BoardDetailDto>>;

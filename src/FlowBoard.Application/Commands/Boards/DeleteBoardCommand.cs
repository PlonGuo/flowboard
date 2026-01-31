using FlowBoard.Application.Common;
using MediatR;

namespace FlowBoard.Application.Commands.Boards;

/// <summary>
/// Command to delete a board.
/// </summary>
public record DeleteBoardCommand(
    int Id,
    int UserId
) : IRequest<Result>;

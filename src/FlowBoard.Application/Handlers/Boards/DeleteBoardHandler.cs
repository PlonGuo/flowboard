using FlowBoard.Application.Commands.Boards;
using FlowBoard.Application.Common;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Boards;

/// <summary>
/// Handler for deleting a board.
/// </summary>
public class DeleteBoardHandler : IRequestHandler<DeleteBoardCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBoardHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteBoardCommand request,
        CancellationToken cancellationToken)
    {
        var board = await _unitOfWork.Boards.GetByIdAsync(request.Id);

        if (board == null)
        {
            return Result.Failure("Board not found");
        }

        // Only board creator or team owner can delete
        var team = await _unitOfWork.Teams.GetByIdAsync(board.TeamId);

        if (board.CreatedById != request.UserId && team?.OwnerId != request.UserId)
        {
            return Result.Failure("You don't have permission to delete this board");
        }

        _unitOfWork.Boards.Remove(board);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

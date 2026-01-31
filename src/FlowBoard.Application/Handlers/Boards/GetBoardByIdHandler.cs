using AutoMapper;
using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using FlowBoard.Application.Queries.Boards;
using FlowBoard.Core.Entities;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Boards;

/// <summary>
/// Handler for getting a board by ID.
/// </summary>
public class GetBoardByIdHandler : IRequestHandler<GetBoardByIdQuery, Result<BoardDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetBoardByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<BoardDetailDto>> Handle(
        GetBoardByIdQuery request,
        CancellationToken cancellationToken)
    {
        var board = request.IncludeTasks
            ? await _unitOfWork.Boards.GetByIdWithColumnsAndTasksAsync(request.Id)
            : await _unitOfWork.Boards.GetByIdWithColumnsAsync(request.Id);

        if (board == null)
        {
            return Result.Failure<BoardDetailDto>("Board not found");
        }

        // Verify user has access
        if (!await UserHasAccessToBoard(board, request.UserId))
        {
            return Result.Failure<BoardDetailDto>("You don't have access to this board");
        }

        return _mapper.Map<BoardDetailDto>(board);
    }

    private async Task<bool> UserHasAccessToBoard(Board board, int userId)
    {
        // Board creator has access
        if (board.CreatedById == userId)
        {
            return true;
        }

        // Team owner has access
        var team = await _unitOfWork.Teams.GetByIdAsync(board.TeamId);
        if (team?.OwnerId == userId)
        {
            return true;
        }

        // Team member has access
        var membership = await _unitOfWork.TeamMembers
            .FindAsync(tm => tm.TeamId == board.TeamId && tm.UserId == userId);

        return membership.Any();
    }
}

using AutoMapper;
using FlowBoard.Application.Commands.Boards;
using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using FlowBoard.Core.Entities;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Boards;

/// <summary>
/// Handler for updating an existing board.
/// </summary>
public class UpdateBoardHandler : IRequestHandler<UpdateBoardCommand, Result<BoardDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateBoardHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<BoardDto>> Handle(
        UpdateBoardCommand request,
        CancellationToken cancellationToken)
    {
        var board = await _unitOfWork.Boards.GetByIdWithColumnsAsync(request.Id);

        if (board == null)
        {
            return Result.Failure<BoardDto>("Board not found");
        }

        // Authorization check
        if (!await CanUserModifyBoard(board, request.UserId))
        {
            return Result.Failure<BoardDto>("You don't have permission to update this board");
        }

        board.Name = request.Name.Trim();
        board.Description = request.Description?.Trim();
        board.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Boards.Update(board);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<BoardDto>(board);
    }

    private async Task<bool> CanUserModifyBoard(Board board, int userId)
    {
        // Board creator can always modify
        if (board.CreatedById == userId)
        {
            return true;
        }

        // Team owner can modify
        var team = await _unitOfWork.Teams.GetByIdAsync(board.TeamId);
        if (team?.OwnerId == userId)
        {
            return true;
        }

        // Team member can modify
        var membership = await _unitOfWork.TeamMembers
            .FindAsync(tm => tm.TeamId == board.TeamId && tm.UserId == userId);

        return membership.Any();
    }
}

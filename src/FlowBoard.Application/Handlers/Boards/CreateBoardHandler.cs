using AutoMapper;
using FlowBoard.Application.Commands.Boards;
using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using FlowBoard.Core.Entities;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Boards;

/// <summary>
/// Handler for creating a new board.
/// </summary>
public class CreateBoardHandler : IRequestHandler<CreateBoardCommand, Result<BoardDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateBoardHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<BoardDetailDto>> Handle(
        CreateBoardCommand request,
        CancellationToken cancellationToken)
    {
        // Verify team exists
        var team = await _unitOfWork.Teams.GetByIdAsync(request.TeamId);
        if (team == null)
        {
            return Result.Failure<BoardDetailDto>("Team not found");
        }

        // Verify user is team owner or member
        if (team.OwnerId != request.UserId)
        {
            var teamMembers = await _unitOfWork.TeamMembers
                .FindAsync(tm => tm.TeamId == request.TeamId && tm.UserId == request.UserId);

            if (!teamMembers.Any())
            {
                return Result.Failure<BoardDetailDto>("You are not a member of this team");
            }
        }

        // Create the board
        var board = new Board
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            TeamId = request.TeamId,
            CreatedById = request.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Boards.AddAsync(board);

        // Create default columns
        var defaultColumns = new[]
        {
            new Column { Name = "To Do", Position = 0, BoardId = board.Id },
            new Column { Name = "In Progress", Position = 1, WipLimit = 5, BoardId = board.Id },
            new Column { Name = "Done", Position = 2, BoardId = board.Id }
        };

        foreach (var column in defaultColumns)
        {
            column.Board = board;
        }

        await _unitOfWork.Columns.AddRangeAsync(defaultColumns);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var createdBoard = await _unitOfWork.Boards.GetByIdWithColumnsAsync(board.Id);
        return _mapper.Map<BoardDetailDto>(createdBoard);
    }
}

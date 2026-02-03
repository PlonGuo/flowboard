using AutoMapper;
using FlowBoard.Application.Commands.Tasks;
using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using FlowBoard.Core.Entities;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Tasks;

/// <summary>
/// Handler for creating a new task.
/// </summary>
public class CreateTaskHandler : IRequestHandler<CreateTaskCommand, Result<TaskItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IBoardNotificationService _notificationService;

    public CreateTaskHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IBoardNotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<Result<TaskItemDto>> Handle(
        CreateTaskCommand request,
        CancellationToken cancellationToken)
    {
        // Get the column
        var column = await _unitOfWork.Columns.GetByIdAsync(request.ColumnId);
        if (column == null)
        {
            return Result.Failure<TaskItemDto>("Column not found");
        }

        // Get the board and verify access
        var board = await _unitOfWork.Boards.GetByIdAsync(column.BoardId);
        if (board == null)
        {
            return Result.Failure<TaskItemDto>("Board not found");
        }

        // Verify user has access to the board's team
        if (!await UserHasAccessToBoard(board, request.UserId))
        {
            return Result.Failure<TaskItemDto>("You don't have access to this board");
        }

        // Verify assignee is a team member (if provided)
        if (request.AssigneeId.HasValue)
        {
            var isAssigneeMember = await IsUserTeamMember(board.TeamId, request.AssigneeId.Value);
            if (!isAssigneeMember)
            {
                return Result.Failure<TaskItemDto>("Assignee must be a team member");
            }
        }

        // Get max position in column and set new task position
        var maxPosition = await _unitOfWork.Tasks.GetMaxPositionInColumnAsync(request.ColumnId);
        var newPosition = maxPosition + 1;

        // Create the task
        var task = new TaskItem
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            ColumnId = request.ColumnId,
            Position = newPosition,
            Priority = request.Priority,
            AssigneeId = request.AssigneeId,
            DueDate = request.DueDate,
            CreatedById = request.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            // Initialize RowVersion for PostgreSQL (doesn't auto-generate like SQL Server)
            RowVersion = BitConverter.GetBytes(DateTime.UtcNow.Ticks)
        };

        await _unitOfWork.Tasks.AddAsync(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var createdTask = await _unitOfWork.Tasks.GetByIdWithCommentsAsync(task.Id);
        var taskDto = _mapper.Map<TaskItemDto>(createdTask);

        // Notify other clients viewing this board
        await _notificationService.NotifyTaskCreatedAsync(
            board.Id,
            taskDto,
            request.ConnectionId);

        return taskDto;
    }

    private async Task<bool> UserHasAccessToBoard(Board board, int userId)
    {
        if (board.CreatedById == userId)
        {
            return true;
        }

        var team = await _unitOfWork.Teams.GetByIdAsync(board.TeamId);
        if (team?.OwnerId == userId)
        {
            return true;
        }

        var membership = await _unitOfWork.TeamMembers
            .FindAsync(tm => tm.TeamId == board.TeamId && tm.UserId == userId);

        return membership.Any();
    }

    private async Task<bool> IsUserTeamMember(int teamId, int userId)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
        if (team?.OwnerId == userId)
        {
            return true;
        }

        var membership = await _unitOfWork.TeamMembers
            .FindAsync(tm => tm.TeamId == teamId && tm.UserId == userId);

        return membership.Any();
    }
}

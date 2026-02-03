using AutoMapper;
using FlowBoard.Application.Commands.Tasks;
using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using FlowBoard.Core.Entities;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Tasks;

/// <summary>
/// Handler for updating an existing task.
/// </summary>
public class UpdateTaskHandler : IRequestHandler<UpdateTaskCommand, Result<TaskItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IBoardNotificationService _notificationService;

    public UpdateTaskHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IBoardNotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<Result<TaskItemDto>> Handle(
        UpdateTaskCommand request,
        CancellationToken cancellationToken)
    {
        var task = await _unitOfWork.Tasks.GetByIdWithCommentsAsync(request.Id);
        if (task == null)
        {
            return Result.Failure<TaskItemDto>("Task not found");
        }

        // Get the board via column
        var board = await _unitOfWork.Boards.GetByIdAsync(task.Column.BoardId);
        if (board == null)
        {
            return Result.Failure<TaskItemDto>("Board not found");
        }

        // Verify user has access
        if (!await UserHasAccessToBoard(board, request.UserId))
        {
            return Result.Failure<TaskItemDto>("You don't have permission to update this task");
        }

        // Check concurrency
        if (!task.RowVersion.SequenceEqual(request.RowVersion))
        {
            return Result.Failure<TaskItemDto>("The task has been modified by another user. Please refresh and try again.");
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

        // Update task fields
        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.Priority = request.Priority;
        task.AssigneeId = request.AssigneeId;
        task.DueDate = request.DueDate;
        task.UpdatedAt = DateTime.UtcNow;
        // Update RowVersion for concurrency control (PostgreSQL doesn't auto-generate)
        task.RowVersion = BitConverter.GetBytes(DateTime.UtcNow.Ticks);

        _unitOfWork.Tasks.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var updatedTask = await _unitOfWork.Tasks.GetByIdWithCommentsAsync(task.Id);
        var taskDto = _mapper.Map<TaskItemDto>(updatedTask);

        // Notify other clients viewing this board
        await _notificationService.NotifyTaskUpdatedAsync(
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

using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using FlowBoard.Application.Queries.Boards;
using FlowBoard.Core.Entities;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Boards;

/// <summary>
/// Handler for getting all team members who can be assigned to tasks on a board.
/// </summary>
public class GetBoardMembersHandler : IRequestHandler<GetBoardMembersQuery, Result<IEnumerable<UserSummaryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBoardMembersHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<UserSummaryDto>>> Handle(
        GetBoardMembersQuery request,
        CancellationToken cancellationToken)
    {
        // Get the board to find its TeamId
        var board = await _unitOfWork.Boards.GetByIdAsync(request.BoardId);
        if (board == null)
        {
            return Result.Failure<IEnumerable<UserSummaryDto>>("Board not found");
        }

        // Verify user has access to this board
        if (!await UserHasAccessToBoard(board, request.UserId))
        {
            return Result.Failure<IEnumerable<UserSummaryDto>>("You don't have access to this board");
        }

        // Get team with all members loaded
        var team = await _unitOfWork.Teams.GetByIdWithMembersAsync(board.TeamId);
        if (team == null)
        {
            return Result.Failure<IEnumerable<UserSummaryDto>>("Team not found");
        }

        // Build list of assignable users (team owner + all team members)
        var members = new List<UserSummaryDto>();

        // Add team owner first
        members.Add(new UserSummaryDto
        {
            Id = team.Owner.Id,
            FullName = team.Owner.FullName,
            AvatarUrl = team.Owner.AvatarUrl
        });

        // Add all team members (excluding owner if they're also in Members collection)
        foreach (var member in team.Members.Where(m => m.UserId != team.OwnerId))
        {
            members.Add(new UserSummaryDto
            {
                Id = member.User.Id,
                FullName = member.User.FullName,
                AvatarUrl = member.User.AvatarUrl
            });
        }

        return members;
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

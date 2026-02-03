using FlowBoard.Application.Commands.Teams;
using FlowBoard.Application.Common;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Teams;

public class LeaveTeamHandler : IRequestHandler<LeaveTeamCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public LeaveTeamHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        LeaveTeamCommand request,
        CancellationToken cancellationToken)
    {
        // Find team
        var team = await _unitOfWork.Teams.GetByIdAsync(request.TeamId);
        if (team == null)
        {
            return Result.Failure("Team not found");
        }

        // Owner cannot leave - must delete team or transfer ownership
        if (team.OwnerId == request.UserId)
        {
            return Result.Failure("Team owner cannot leave the team. Delete the team or transfer ownership first.");
        }

        // Find membership
        var memberships = await _unitOfWork.TeamMembers.FindAsync(
            m => m.TeamId == request.TeamId && m.UserId == request.UserId);
        var membership = memberships.FirstOrDefault();

        if (membership == null)
        {
            return Result.Failure("You are not a member of this team");
        }

        // Remove membership
        _unitOfWork.TeamMembers.Remove(membership);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

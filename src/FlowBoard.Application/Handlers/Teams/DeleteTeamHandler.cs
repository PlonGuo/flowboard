using FlowBoard.Application.Commands.Teams;
using FlowBoard.Application.Common;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Teams;

public class DeleteTeamHandler : IRequestHandler<DeleteTeamCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTeamHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteTeamCommand request,
        CancellationToken cancellationToken)
    {
        // Find team
        var team = await _unitOfWork.Teams.GetByIdAsync(request.TeamId);
        if (team == null)
        {
            return Result.Failure("Team not found");
        }

        // Verify user is the team owner
        if (team.OwnerId != request.UserId)
        {
            return Result.Failure("Only the team owner can delete a team");
        }

        // Delete team (cascade will handle boards, columns, tasks, and members)
        _unitOfWork.Teams.Remove(team);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

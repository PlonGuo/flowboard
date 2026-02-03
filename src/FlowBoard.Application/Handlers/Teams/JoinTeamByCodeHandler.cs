using AutoMapper;
using FlowBoard.Application.Commands.Teams;
using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using FlowBoard.Core.Entities;
using FlowBoard.Core.Enums;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Teams;

public class JoinTeamByCodeHandler : IRequestHandler<JoinTeamByCodeCommand, Result<TeamDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public JoinTeamByCodeHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<TeamDto>> Handle(
        JoinTeamByCodeCommand request,
        CancellationToken cancellationToken)
    {
        // Normalize invite code to uppercase
        var normalizedCode = request.InviteCode.ToUpperInvariant().Trim();

        // Find team by invite code
        var team = await _unitOfWork.Teams.GetByInviteCodeAsync(normalizedCode);
        if (team == null)
        {
            return Result.Failure<TeamDto>("Invalid invite code");
        }

        // Check if user is already a member
        var isAlreadyMember = await _unitOfWork.Teams.IsUserMemberAsync(team.Id, request.UserId);
        if (isAlreadyMember)
        {
            return Result.Failure<TeamDto>("You are already a member of this team");
        }

        // Verify user exists
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return Result.Failure<TeamDto>("User not found");
        }

        // Add user as team member
        var teamMember = new TeamMember
        {
            TeamId = team.Id,
            UserId = request.UserId,
            Role = TeamRole.Member,
            JoinedAt = DateTime.UtcNow
        };

        await _unitOfWork.TeamMembers.AddAsync(teamMember);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload team with updated members
        var updatedTeam = await _unitOfWork.Teams.GetByIdWithMembersAsync(team.Id);
        return _mapper.Map<TeamDto>(updatedTeam);
    }
}

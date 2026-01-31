using AutoMapper;
using FlowBoard.Application.Commands.Teams;
using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using FlowBoard.Core.Entities;
using FlowBoard.Core.Enums;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Teams;

public class CreateTeamHandler : IRequestHandler<CreateTeamCommand, Result<TeamDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateTeamHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<TeamDto>> Handle(
        CreateTeamCommand request,
        CancellationToken cancellationToken)
    {
        // Verify user exists
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return Result.Failure<TeamDto>("User not found");
        }

        // Create the team
        var team = new Team
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            OwnerId = request.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Teams.AddAsync(team);

        // Add the owner as a member with Owner role
        var ownerMember = new TeamMember
        {
            Team = team,
            UserId = request.UserId,
            Role = TeamRole.Owner,
            JoinedAt = DateTime.UtcNow
        };

        await _unitOfWork.TeamMembers.AddAsync(ownerMember);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var createdTeam = await _unitOfWork.Teams.GetByIdWithMembersAsync(team.Id);
        return _mapper.Map<TeamDto>(createdTeam);
    }
}

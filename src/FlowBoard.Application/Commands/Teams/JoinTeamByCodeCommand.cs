using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using MediatR;

namespace FlowBoard.Application.Commands.Teams;

public record JoinTeamByCodeCommand(
    string InviteCode,
    int UserId
) : IRequest<Result<TeamDto>>;

using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using MediatR;

namespace FlowBoard.Application.Commands.Teams;

public record CreateTeamCommand(
    string Name,
    string? Description,
    int UserId
) : IRequest<Result<TeamDto>>;

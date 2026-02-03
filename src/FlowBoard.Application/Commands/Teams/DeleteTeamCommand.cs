using FlowBoard.Application.Common;
using MediatR;

namespace FlowBoard.Application.Commands.Teams;

public record DeleteTeamCommand(
    int TeamId,
    int UserId
) : IRequest<Result>;

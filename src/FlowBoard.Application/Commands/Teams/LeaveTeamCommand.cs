using FlowBoard.Application.Common;
using MediatR;

namespace FlowBoard.Application.Commands.Teams;

public record LeaveTeamCommand(
    int TeamId,
    int UserId
) : IRequest<Result>;

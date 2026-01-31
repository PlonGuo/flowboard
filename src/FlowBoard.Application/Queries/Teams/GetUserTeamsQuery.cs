using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using MediatR;

namespace FlowBoard.Application.Queries.Teams;

public record GetUserTeamsQuery(int UserId) : IRequest<Result<IEnumerable<TeamDto>>>;

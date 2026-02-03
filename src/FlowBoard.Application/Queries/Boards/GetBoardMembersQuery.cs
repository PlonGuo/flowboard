using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using MediatR;

namespace FlowBoard.Application.Queries.Boards;

/// <summary>
/// Query to get all team members who can be assigned to tasks on a board.
/// </summary>
public record GetBoardMembersQuery(
    int BoardId,
    int UserId
) : IRequest<Result<IEnumerable<UserSummaryDto>>>;

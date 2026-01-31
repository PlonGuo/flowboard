using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using MediatR;

namespace FlowBoard.Application.Queries.Boards;

/// <summary>
/// Query to get all boards accessible to a user.
/// </summary>
public record GetUserBoardsQuery(
    int UserId
) : IRequest<Result<IEnumerable<BoardSummaryDto>>>;

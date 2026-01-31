using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using MediatR;

namespace FlowBoard.Application.Queries.Boards;

/// <summary>
/// Query to get a board by its ID.
/// </summary>
public record GetBoardByIdQuery(
    int Id,
    int UserId,
    bool IncludeTasks = true
) : IRequest<Result<BoardDetailDto>>;

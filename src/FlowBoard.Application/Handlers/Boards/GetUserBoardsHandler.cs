using AutoMapper;
using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using FlowBoard.Application.Queries.Boards;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Boards;

/// <summary>
/// Handler for getting all boards accessible to a user.
/// </summary>
public class GetUserBoardsHandler : IRequestHandler<GetUserBoardsQuery, Result<IEnumerable<BoardSummaryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUserBoardsHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<BoardSummaryDto>>> Handle(
        GetUserBoardsQuery request,
        CancellationToken cancellationToken)
    {
        var boards = await _unitOfWork.Boards.GetByUserIdAsync(request.UserId);
        return Result.Success(_mapper.Map<IEnumerable<BoardSummaryDto>>(boards));
    }
}

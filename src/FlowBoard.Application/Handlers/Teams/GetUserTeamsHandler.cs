using AutoMapper;
using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using FlowBoard.Application.Queries.Teams;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Teams;

public class GetUserTeamsHandler : IRequestHandler<GetUserTeamsQuery, Result<IEnumerable<TeamDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUserTeamsHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<TeamDto>>> Handle(
        GetUserTeamsQuery request,
        CancellationToken cancellationToken)
    {
        var teams = await _unitOfWork.Teams.GetByUserIdAsync(request.UserId);
        var teamDtos = _mapper.Map<IEnumerable<TeamDto>>(teams);
        return Result.Success(teamDtos);
    }
}

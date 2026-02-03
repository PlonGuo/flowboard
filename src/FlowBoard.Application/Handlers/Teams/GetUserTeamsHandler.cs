using AutoMapper;
using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using FlowBoard.Application.Queries.Teams;
using FlowBoard.Core.Enums;
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
        var teamDtos = teams.Select(team =>
        {
            var dto = _mapper.Map<TeamDto>(team);
            // Set CurrentUserRole based on ownership
            var role = team.OwnerId == request.UserId ? TeamRole.Owner : TeamRole.Member;
            return dto with { CurrentUserRole = role };
        }).ToList();
        return Result.Success<IEnumerable<TeamDto>>(teamDtos);
    }
}

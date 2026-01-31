using FlowBoard.Application.Commands.Teams;
using FlowBoard.Application.DTOs;
using FlowBoard.Application.Queries.Teams;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.API.Controllers;

/// <summary>
/// Controller for team management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeamsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TeamsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Get all teams the current user belongs to.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TeamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<TeamDto>>> GetUserTeams()
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var query = new GetUserTeamsQuery(userId);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new team.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TeamDto>> CreateTeam([FromBody] CreateTeamRequest request)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var command = new CreateTeamCommand(
            request.Name,
            request.Description,
            userId
        );

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }

        return CreatedAtAction(nameof(GetUserTeams), result.Value);
    }
}

/// <summary>
/// Request model for creating a team.
/// </summary>
public record CreateTeamRequest(string Name, string? Description);

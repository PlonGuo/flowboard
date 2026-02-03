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

    /// <summary>
    /// Join a team using an invite code.
    /// </summary>
    [HttpPost("join")]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TeamDto>> JoinTeam([FromBody] JoinTeamRequest request)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var command = new JoinTeamByCodeCommand(
            request.InviteCode,
            userId
        );

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Delete a team (owner only). Deletes all associated boards and tasks.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteTeam(int id)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var command = new DeleteTeamCommand(id, userId);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            if (result.Error!.Contains("not found"))
            {
                return NotFound(new { message = result.Error });
            }
            if (result.Error.Contains("owner"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = result.Error });
            }
            return BadRequest(new { message = result.Error });
        }

        return NoContent();
    }

    /// <summary>
    /// Leave a team (members only). Owners must delete or transfer ownership.
    /// </summary>
    [HttpPost("{id}/leave")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LeaveTeam(int id)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var command = new LeaveTeamCommand(id, userId);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            if (result.Error!.Contains("not found"))
            {
                return NotFound(new { message = result.Error });
            }
            return BadRequest(new { message = result.Error });
        }

        return NoContent();
    }
}

/// <summary>
/// Request model for creating a team.
/// </summary>
public record CreateTeamRequest(string Name, string? Description);

/// <summary>
/// Request model for joining a team by invite code.
/// </summary>
public record JoinTeamRequest(string InviteCode);

using FlowBoard.Application.Commands.Boards;
using FlowBoard.Application.DTOs;
using FlowBoard.Application.Queries.Boards;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.API.Controllers;

/// <summary>
/// Controller for board management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BoardsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BoardsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Get all boards accessible to the current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BoardSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<BoardSummaryDto>>> GetUserBoards()
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var query = new GetUserBoardsQuery(userId);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get a specific board by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BoardDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BoardDetailDto>> GetBoard(int id, [FromQuery] bool includeTasks = true)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var query = new GetBoardByIdQuery(id, userId, includeTasks);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            if (result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { message = result.Error });
            }
            if (result.Error.Contains("access", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }
            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new board.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BoardDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BoardDetailDto>> CreateBoard([FromBody] CreateBoardRequest request)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var command = new CreateBoardCommand(
            request.Name,
            request.Description,
            request.TeamId,
            userId
        );

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }

        return CreatedAtAction(nameof(GetBoard), new { id = result.Value.Id }, result.Value);
    }

    /// <summary>
    /// Update an existing board.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(BoardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BoardDto>> UpdateBoard(int id, [FromBody] UpdateBoardRequest request)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var command = new UpdateBoardCommand(id, request.Name, request.Description, userId);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            if (result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { message = result.Error });
            }
            if (result.Error.Contains("permission", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }
            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Delete a board.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteBoard(int id)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var command = new DeleteBoardCommand(id, userId);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            if (result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { message = result.Error });
            }
            if (result.Error.Contains("permission", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }
            return BadRequest(new { message = result.Error });
        }

        return NoContent();
    }
}

/// <summary>
/// Request model for creating a board.
/// </summary>
public record CreateBoardRequest(string Name, string? Description, int TeamId);

/// <summary>
/// Request model for updating a board.
/// </summary>
public record UpdateBoardRequest(string Name, string? Description);

using FlowBoard.Application.Commands.Canvas;
using FlowBoard.Application.DTOs;
using FlowBoard.Application.Queries.Canvas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.API.Controllers;

/// <summary>
/// Controller for canvas management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CanvasesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CanvasesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private string? GetConnectionId()
    {
        return Request.Headers["X-SignalR-Connection-Id"].FirstOrDefault();
    }

    /// <summary>
    /// Get canvas for a specific task.
    /// Returns null if task has no canvas.
    /// </summary>
    [HttpGet("task/{taskId:int}")]
    [ProducesResponseType(typeof(CanvasDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CanvasDetailDto?>> GetTaskCanvas(int taskId)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var query = new GetTaskCanvasQuery(taskId, userId);
        var result = await _mediator.Send(query);

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

        // Return null as 204 No Content if no canvas exists
        if (result.Value == null)
        {
            return NoContent();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Create a canvas for a task.
    /// </summary>
    [HttpPost("task/{taskId:int}")]
    [ProducesResponseType(typeof(CanvasDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CanvasDetailDto>> CreateTaskCanvas(
        int taskId,
        [FromBody] CreateTaskCanvasRequest request)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var command = new CreateTaskCanvasCommand(
            taskId,
            request.Name,
            userId);

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

        return CreatedAtAction(nameof(GetCanvas), new { id = result.Value.Id }, result.Value);
    }

    /// <summary>
    /// Get a canvas by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CanvasDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CanvasDetailDto>> GetCanvas(int id)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var query = new GetCanvasByIdQuery(id, userId);
        var result = await _mediator.Send(query);

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
    /// Update canvas data (elements, appState, files).
    /// </summary>
    [HttpPut("{id:int}/data")]
    [ProducesResponseType(typeof(SaveCanvasDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SaveCanvasDataResponse>> UpdateCanvasData(
        int id,
        [FromBody] UpdateCanvasDataRequest request)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var command = new UpdateCanvasDataCommand(
            id,
            request.Elements,
            request.AppState,
            request.Files,
            userId,
            GetConnectionId());

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
    /// Delete a canvas.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteCanvas(int id)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var command = new DeleteCanvasCommand(id, userId);
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
/// Request model for creating a task canvas.
/// </summary>
public record CreateTaskCanvasRequest(string Name);

/// <summary>
/// Request model for updating canvas data.
/// </summary>
public record UpdateCanvasDataRequest(
    string Elements,
    string? AppState = null,
    string? Files = null);

using FlowBoard.Application.Commands.Comments;
using FlowBoard.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.API.Controllers;

/// <summary>
/// Controller for comment management operations.
/// </summary>
[ApiController]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CommentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Get the SignalR connection ID from request header (for excluding sender from notifications).
    /// </summary>
    private string? GetConnectionId()
    {
        return Request.Headers["X-SignalR-Connection-Id"].FirstOrDefault();
    }

    /// <summary>
    /// Create a new comment on a task.
    /// </summary>
    [HttpPost("api/tasks/{taskId:int}/comments")]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CommentDto>> CreateComment(int taskId, [FromBody] CreateCommentRequest request)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var connectionId = GetConnectionId();

        var command = new CreateCommentCommand(
            request.Content,
            taskId,
            userId,
            connectionId
        );

        var result = await _mediator.Send(command);

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

        return CreatedAtAction(nameof(CreateComment), new { taskId, id = result.Value.Id }, result.Value);
    }

    /// <summary>
    /// Update an existing comment.
    /// </summary>
    [HttpPut("api/comments/{id:int}")]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CommentDto>> UpdateComment(int id, [FromBody] UpdateCommentRequest request)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var connectionId = GetConnectionId();

        var command = new UpdateCommentCommand(
            id,
            request.Content,
            userId,
            connectionId
        );

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            if (result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { message = result.Error });
            }
            if (result.Error.Contains("only edit your own", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }
            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Delete a comment.
    /// </summary>
    [HttpDelete("api/comments/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var connectionId = GetConnectionId();

        var command = new DeleteCommentCommand(
            id,
            userId,
            connectionId
        );

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
/// Request model for creating a comment.
/// </summary>
public record CreateCommentRequest(
    string Content
);

/// <summary>
/// Request model for updating a comment.
/// </summary>
public record UpdateCommentRequest(
    string Content
);

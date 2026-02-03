using FlowBoard.Application.Commands.Tasks;
using FlowBoard.Application.DTOs;
using FlowBoard.Application.Queries.Tasks;
using FlowBoard.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.API.Controllers;

/// <summary>
/// Controller for task management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
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
    /// Get a specific task by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaskItemDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TaskItemDetailDto>> GetTask(int id, [FromQuery] bool includeComments = true)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var query = new GetTaskByIdQuery(id, userId, includeComments);
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
    /// Create a new task.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TaskItemDto>> CreateTask([FromBody] CreateTaskRequest request)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var priority = ParsePriority(request.Priority);
        var connectionId = GetConnectionId();

        var command = new CreateTaskCommand(
            request.Title,
            request.Description,
            request.ColumnId,
            priority,
            request.AssigneeId,
            request.DueDate,
            userId,
            connectionId
        );

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            if (result.Error!.Contains("access", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }
            return BadRequest(new { message = result.Error });
        }

        return CreatedAtAction(nameof(GetTask), new { id = result.Value.Id }, result.Value);
    }

    /// <summary>
    /// Update an existing task.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TaskItemDto>> UpdateTask(int id, [FromBody] UpdateTaskRequest request)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var priority = ParsePriority(request.Priority);
        var rowVersion = Convert.FromBase64String(request.RowVersion);
        var connectionId = GetConnectionId();

        var command = new UpdateTaskCommand(
            id,
            request.Title,
            request.Description,
            priority,
            request.AssigneeId,
            request.DueDate,
            userId,
            rowVersion,
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
            if (result.Error.Contains("modified by another", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new { message = result.Error });
            }
            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Delete a task.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var connectionId = GetConnectionId();
        var command = new DeleteTaskCommand(id, userId, connectionId);
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

    /// <summary>
    /// Move a task to a different column and/or position.
    /// </summary>
    [HttpPut("{id:int}/move")]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TaskItemDto>> MoveTask(int id, [FromBody] MoveTaskRequest request)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var rowVersion = Convert.FromBase64String(request.RowVersion);
        var connectionId = GetConnectionId();

        var command = new MoveTaskCommand(
            id,
            request.ToColumnId,
            request.ToPosition,
            userId,
            rowVersion,
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
            if (result.Error.Contains("modified by another", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new { message = result.Error });
            }
            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    private static TaskPriority ParsePriority(string? priority)
    {
        if (string.IsNullOrEmpty(priority))
        {
            return TaskPriority.Medium;
        }

        return Enum.TryParse<TaskPriority>(priority, ignoreCase: true, out var result)
            ? result
            : TaskPriority.Medium;
    }
}

/// <summary>
/// Request model for creating a task.
/// </summary>
public record CreateTaskRequest(
    string Title,
    string? Description,
    int ColumnId,
    string? Priority,
    int? AssigneeId,
    DateTime? DueDate
);

/// <summary>
/// Request model for updating a task.
/// </summary>
public record UpdateTaskRequest(
    string Title,
    string? Description,
    string Priority,
    int? AssigneeId,
    DateTime? DueDate,
    string RowVersion
);

/// <summary>
/// Request model for moving a task.
/// </summary>
public record MoveTaskRequest(
    int ToColumnId,
    int ToPosition,
    string RowVersion
);

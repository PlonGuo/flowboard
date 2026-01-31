using FlowBoard.Application.Commands.Auth;
using FlowBoard.Application.DTOs;
using FlowBoard.Core.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.API.Controllers;

/// <summary>
/// Authentication controller for login, registration, and profile management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AuthController(IMediator mediator, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Authenticate user with email and password.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return Unauthorized(new { message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Register a new user account.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            if (result.Error!.Contains("already registered", StringComparison.OrdinalIgnoreCase))
                return Conflict(new { message = result.Error });
            return BadRequest(new { message = result.Error });
        }

        return CreatedAtAction(nameof(GetCurrentUser), result.Value);
    }

    /// <summary>
    /// Get current authenticated user's profile.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        return Ok(_mapper.Map<UserDto>(user));
    }

    /// <summary>
    /// Update current user's profile.
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var command = new UpdateProfileCommand(userId, request.FullName, request.AvatarUrl);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(new { message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get list of available default avatars.
    /// </summary>
    [HttpGet("avatars")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    public ActionResult<string[]> GetDefaultAvatars()
    {
        var avatars = Enumerable.Range(1, 8)
            .Select(i => $"/assets/avatars/avatar-{i}.svg")
            .ToArray();
        return Ok(avatars);
    }
}

/// <summary>
/// Request model for profile updates.
/// </summary>
public record UpdateProfileRequest(string FullName, string? AvatarUrl);

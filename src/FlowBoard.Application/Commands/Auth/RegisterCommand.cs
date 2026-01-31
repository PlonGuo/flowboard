using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using MediatR;

namespace FlowBoard.Application.Commands.Auth;

/// <summary>
/// Command for user registration.
/// </summary>
public record RegisterCommand(
    string Email,
    string Password,
    string FullName,
    string? AvatarUrl = null
) : IRequest<Result<AuthResponseDto>>;

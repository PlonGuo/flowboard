using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using MediatR;

namespace FlowBoard.Application.Commands.Auth;

/// <summary>
/// Command for updating user profile.
/// </summary>
public record UpdateProfileCommand(
    int UserId,
    string FullName,
    string? AvatarUrl
) : IRequest<Result<UserDto>>;

using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using MediatR;

namespace FlowBoard.Application.Commands.Auth;

/// <summary>
/// Command for user login with email and password.
/// </summary>
public record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponseDto>>;

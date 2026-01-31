using AutoMapper;
using FlowBoard.Application.Commands.Auth;
using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Auth;

/// <summary>
/// Handler for user login command.
/// </summary>
public class LoginHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    public LoginHandler(
        IUnitOfWork unitOfWork,
        IAuthService authService,
        IPasswordHasher passwordHasher,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    public async Task<Result<AuthResponseDto>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result.Failure<AuthResponseDto>("Invalid email or password");
        }

        if (!user.IsActive)
        {
            return Result.Failure<AuthResponseDto>("Account is inactive");
        }

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _authService.GenerateAccessToken(user);
        var refreshToken = _authService.GenerateRefreshToken();

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_authService.GetTokenExpirationMinutes()),
            User = _mapper.Map<UserDto>(user)
        };
    }
}

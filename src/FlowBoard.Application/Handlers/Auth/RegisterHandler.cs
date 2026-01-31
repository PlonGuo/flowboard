using AutoMapper;
using FlowBoard.Application.Commands.Auth;
using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using FlowBoard.Core.Entities;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Auth;

/// <summary>
/// Handler for user registration command.
/// </summary>
public class RegisterHandler : IRequestHandler<RegisterCommand, Result<AuthResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    public RegisterHandler(
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
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
        {
            return Result.Failure<AuthResponseDto>("Email already registered");
        }

        var user = new User
        {
            Email = request.Email.ToLower().Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            FullName = request.FullName.Trim(),
            AvatarUrl = request.AvatarUrl ?? "/assets/avatars/avatar-1.svg",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _unitOfWork.Users.AddAsync(user);
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

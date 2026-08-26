using System.ComponentModel.DataAnnotations;
using KayraExport.Application.Auth.Dtos;
using KayraExport.Application.Interfaces;
using MediatR;

namespace KayraExport.Application.Auth.Commands.Login;

public sealed class LoginCommand : IRequest<AuthResponse>
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

public sealed class LoginCommandHandler
    : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(
            normalizedEmail,
            cancellationToken);

        if (user is null ||
            !_passwordHasher.Verify(
                request.Password,
                user.PasswordHash))
        {
            throw new UnauthorizedAccessException(
                "Email or password is incorrect.");
        }

        var token = _tokenService.GenerateToken(user);

        return new AuthResponse(
            user.Id,
            user.Email,
            token);
    }
}
using System.ComponentModel.DataAnnotations;
using KayraExport.Application.Auth.Dtos;
using KayraExport.Application.Interfaces;
using KayraExport.Core.Entities;
using MediatR;

namespace KayraExport.Application.Auth.Commands.Register;

public sealed class RegisterCommand : IRequest<AuthResponse>
{
    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public string Password { get; init; } = string.Empty;
}

public sealed class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailExists = await _userRepository.EmailExistsAsync(
            normalizedEmail,
            cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException(
                "A user with this email address already exists.");
        }

        var user = new AppUser
        {
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(request.Password)
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var token = _tokenService.GenerateToken(user);

        return new AuthResponse(
            user.Id,
            user.Email,
            token);
    }
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using KayraExport.Auth.Application.Contracts;
using KayraExport.Auth.Application.Interfaces;
using KayraExport.Auth.Core.Entities;
using KayraExport.Auth.Infrastructure.Authentication;
using KayraExport.Auth.Infrastructure.Identity;
using KayraExport.Auth.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace KayraExport.Auth.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private const string DefaultRole = "User";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly AuthDbContext _dbContext;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        AuthDbContext dbContext,
        IOptions<JwtSettings> jwtOptions)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
        _jwtSettings = jwtOptions.Value;

        if (string.IsNullOrWhiteSpace(_jwtSettings.Key) ||
            _jwtSettings.Key.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT key must contain at least 32 characters.");
        }
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();

        var existingUser =
            await _userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            throw new InvalidOperationException(
                "A user with this email already exists.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            FullName = request.FullName.Trim(),
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(
            user,
            request.Password);

        EnsureIdentitySucceeded(createResult);

        await EnsureDefaultRoleExistsAsync();

        var roleResult = await _userManager.AddToRoleAsync(
            user,
            DefaultRole);

        EnsureIdentitySucceeded(roleResult);

        return await CreateAuthResponseAsync(
            user,
            null,
            cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(
            request.Email.Trim());

        if (user is null ||
            !await _userManager.CheckPasswordAsync(
                user,
                request.Password))
        {
            throw new UnauthorizedAccessException(
                "Email or password is incorrect.");
        }

        return await CreateAuthResponseAsync(
            user,
            null,
            cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(
            request.RefreshToken.Trim());

        var storedToken = await _dbContext.RefreshTokens
            .SingleOrDefaultAsync(
                token => token.Token == tokenHash,
                cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Refresh token is invalid or expired.");
        }

        var user = await _userManager.FindByIdAsync(
            storedToken.UserId.ToString());

        if (user is null)
        {
            throw new UnauthorizedAccessException(
                "Refresh token user was not found.");
        }

        return await CreateAuthResponseAsync(
            user,
            storedToken,
            cancellationToken);
    }

    public async Task RevokeAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(
            request.RefreshToken.Trim());

        var storedToken = await _dbContext.RefreshTokens
            .SingleOrDefaultAsync(
                token => token.Token == tokenHash,
                cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
        {
            return;
        }

        storedToken.RevokedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(
        ApplicationUser user,
        RefreshToken? tokenToReplace,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var accessTokenExpiresAt = now.AddMinutes(
            _jwtSettings.AccessTokenMinutes);

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),
            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),
            new(
                JwtRegisteredClaimNames.Email,
                user.Email ?? string.Empty),
            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),
            new(
                ClaimTypes.Name,
                user.FullName)
        };

        claims.AddRange(
            roles.Select(role =>
                new Claim(ClaimTypes.Role, role)));

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Key));

        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: now,
            expires: accessTokenExpiresAt,
            signingCredentials: credentials);

        var accessToken =
            new JwtSecurityTokenHandler().WriteToken(jwt);

        var rawRefreshToken = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64));

        var refreshTokenHash =
            HashToken(rawRefreshToken);

        if (tokenToReplace is not null)
        {
            tokenToReplace.RevokedAt = now;
            tokenToReplace.ReplacedByToken =
                refreshTokenHash;
        }

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenHash,
            CreatedAt = now,
            ExpiresAt = now.AddDays(
                _jwtSettings.RefreshTokenDays)
        });

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new AuthResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.FullName,
            accessToken,
            rawRefreshToken,
            accessTokenExpiresAt);
    }

    private async Task EnsureDefaultRoleExistsAsync()
    {
        if (await _roleManager.RoleExistsAsync(DefaultRole))
        {
            return;
        }

        var result = await _roleManager.CreateAsync(
            new IdentityRole<Guid>
            {
                Id = Guid.NewGuid(),
                Name = DefaultRole
            });

        EnsureIdentitySucceeded(result);
    }

    private static string HashToken(string token)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToBase64String(hashBytes);
    }

    private static void EnsureIdentitySucceeded(
        IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(
            " ",
            result.Errors.Select(error =>
                error.Description));

        throw new ArgumentException(errors);
    }
}
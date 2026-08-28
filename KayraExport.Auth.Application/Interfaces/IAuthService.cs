using KayraExport.Auth.Application.Contracts;

namespace KayraExport.Auth.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default);

    Task RevokeAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default);
}
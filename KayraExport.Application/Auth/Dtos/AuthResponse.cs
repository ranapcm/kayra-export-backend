namespace KayraExport.Application.Auth.Dtos;

public sealed record AuthResponse(
    Guid UserId,
    string Email,
    string Token);
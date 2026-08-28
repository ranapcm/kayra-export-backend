using System.ComponentModel.DataAnnotations;

namespace KayraExport.Auth.Application.Contracts;

public sealed class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; init; } = string.Empty;
}
namespace KayraExport.Auth.Infrastructure.Authentication;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Key { get; init; } = string.Empty;

    public string Issuer { get; init; } =
        "KayraExport.Auth";

    public string Audience { get; init; } =
        "KayraExport.Services";

    public int AccessTokenMinutes { get; init; } = 15;

    public int RefreshTokenDays { get; init; } = 7;
}
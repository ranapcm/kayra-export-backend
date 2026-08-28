using KayraExport.Auth.Application.Contracts;
using KayraExport.Auth.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KayraExport.Auth.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterAsync(
            request,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(
            request,
            cancellationToken);

        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.RefreshAsync(
            request,
            cancellationToken);

        return Ok(response);
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        await _authService.RevokeAsync(
            request,
            cancellationToken);

        return NoContent();
    }
}
using MediRoute.API.DTOs;
using MediRoute.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MediRoute.API.Controllers;

[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) { _auth = auth; }

    /// <summary>Authenticate with username/password and receive a JWT access + refresh token.</summary>
    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto req, CancellationToken ct) =>
        ToActionResult(await _auth.LoginAsync(req, ct));

    /// <summary>Exchange a valid refresh token for a new access + refresh pair.</summary>
    [HttpPost("refresh")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshRequestDto req, CancellationToken ct) =>
        ToActionResult(await _auth.RefreshAsync(req.RefreshToken, ct));
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MediRoute.API.Common;
using MediRoute.API.DTOs;
using MediRoute.API.Models;
using MediRoute.API.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace MediRoute.API.Services;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto req, CancellationToken ct = default);
    Task<Result<AuthResponseDto>> RefreshAsync(string refreshToken, CancellationToken ct = default);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository users, IConfiguration config, ILogger<AuthService> logger)
    {
        _users = users;
        _config = config;
        _logger = logger;
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto req, CancellationToken ct = default)
    {
        var user = await _users.GetByUsernameAsync(req.Username, ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login for {User}", req.Username);
            return Result<AuthResponseDto>.Failure("Invalid credentials", 401);
        }
        return Result<AuthResponseDto>.Success(await IssueTokensAsync(user, ct));
    }

    public async Task<Result<AuthResponseDto>> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var user = await _users.GetByRefreshTokenAsync(refreshToken, ct);
        if (user is null || user.RefreshTokenExpiresAt is null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
            return Result<AuthResponseDto>.Failure("Invalid or expired refresh token", 401);
        return Result<AuthResponseDto>.Success(await IssueTokensAsync(user, ct));
    }

    private async Task<AuthResponseDto> IssueTokensAsync(User user, CancellationToken ct)
    {
        var accessMinutes = _config.GetValue<int?>("JWT:AccessTokenExpiryMinutes") ?? 15;
        var refreshDays = _config.GetValue<int?>("JWT:RefreshTokenExpiryDays") ?? 7;

        var (token, expires) = GenerateAccessToken(user, accessMinutes);
        var refresh = GenerateRefreshToken();

        user.RefreshToken = refresh;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(refreshDays);
        await _users.UpdateAsync(user, ct);

        return new AuthResponseDto(token, refresh, expires, user.Username, user.Role, user.HospitalId);
    }

    private (string Token, DateTime Expires) GenerateAccessToken(User user, int minutes)
    {
        var secret = _config["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret missing");
        var issuer = _config["JWT:Issuer"];
        var audience = _config["JWT:Audience"];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role.ToString())
        };
        if (user.HospitalId.HasValue)
            claims.Add(new Claim("hospitalId", user.HospitalId.Value.ToString()));

        var expires = DateTime.UtcNow.AddMinutes(minutes);
        var jwt = new JwtSecurityToken(issuer, audience, claims, expires: expires, signingCredentials: creds);
        return (new JwtSecurityTokenHandler().WriteToken(jwt), expires);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}

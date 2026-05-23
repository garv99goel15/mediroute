using MediRoute.API.Models;

namespace MediRoute.API.DTOs;

public record LoginRequestDto(string Username, string Password);

public record RefreshRequestDto(string RefreshToken);

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    string Username,
    UserRole Role,
    int? HospitalId
);

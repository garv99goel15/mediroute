using System.ComponentModel.DataAnnotations;

namespace MediRoute.API.Models;

public class User
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string Username { get; set; } = string.Empty;
    [Required] public string PasswordHash { get; set; } = string.Empty;
    public int? HospitalId { get; set; }
    public Hospital? Hospital { get; set; }
    public UserRole Role { get; set; } = UserRole.HospitalAdmin;

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
}

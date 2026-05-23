using System.ComponentModel.DataAnnotations;

namespace MediRoute.API.Models;

public class Bed
{
    public int Id { get; set; }
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    [Required, MaxLength(20)] public string BedNumber { get; set; } = string.Empty;
    public BedStatus Status { get; set; } = BedStatus.Available;
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(100)] public string? LastUpdatedBy { get; set; }
}

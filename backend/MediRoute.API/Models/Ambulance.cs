using System.ComponentModel.DataAnnotations;

namespace MediRoute.API.Models;

public class Ambulance
{
    public int Id { get; set; }
    public int HospitalId { get; set; }
    public Hospital? Hospital { get; set; }

    [Required, MaxLength(20)] public string VehicleNumber { get; set; } = string.Empty;
    public AmbulanceStatus Status { get; set; } = AmbulanceStatus.Available;
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}

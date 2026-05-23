using System.ComponentModel.DataAnnotations;

namespace MediRoute.API.Models;

public class Hospital
{
    public int Id { get; set; }
    [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(300)] public string Address { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string City { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string State { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Long { get; set; }
    [MaxLength(20)] public string Phone { get; set; } = string.Empty;
    [MaxLength(150)] public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Department> Departments { get; set; } = new List<Department>();
    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    public ICollection<Ambulance> Ambulances { get; set; } = new List<Ambulance>();
    public ICollection<ResourceSnapshot> Snapshots { get; set; } = new List<ResourceSnapshot>();
}

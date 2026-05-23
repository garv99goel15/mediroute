using System.ComponentModel.DataAnnotations;

namespace MediRoute.API.Models;

public class Doctor
{
    public int Id { get; set; }
    public int HospitalId { get; set; }
    public Hospital? Hospital { get; set; }

    [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Specialization { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public DateTime? NextAvailableAt { get; set; }
    public decimal ConsultationFee { get; set; }
    public int YearsOfExperience { get; set; }
}

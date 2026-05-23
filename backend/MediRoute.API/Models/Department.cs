using System.ComponentModel.DataAnnotations;

namespace MediRoute.API.Models;

public class Department
{
    public int Id { get; set; }
    public int HospitalId { get; set; }
    public Hospital? Hospital { get; set; }

    [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;
    public DepartmentType Type { get; set; }
    public int TotalBeds { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Bed> Beds { get; set; } = new List<Bed>();
}

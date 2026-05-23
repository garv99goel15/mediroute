namespace MediRoute.API.Models;

public class ResourceSnapshot
{
    public int Id { get; set; }
    public int HospitalId { get; set; }
    public Hospital? Hospital { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public int ICUAvailable { get; set; }
    public int ICUTotal { get; set; }
    public int GeneralAvailable { get; set; }
    public int GeneralTotal { get; set; }
    public int EmergencyAvailable { get; set; }
    public int OTAvailable { get; set; }
}

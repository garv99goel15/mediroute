namespace MediRoute.API.DTOs;

public record AdminDashboardDto(
    int HospitalId,
    string HospitalName,
    int TotalBeds,
    int AvailableBeds,
    int OccupiedBeds,
    int MaintenanceBeds,
    int DoctorsTotal,
    int DoctorsAvailable,
    int AmbulancesTotal,
    int AmbulancesAvailable,
    AvailabilityDto Availability,
    List<RecentActivityDto> RecentActivity
);

public record RecentActivityDto(
    string ResourceType,
    string Description,
    DateTime Timestamp,
    string? UpdatedBy
);

public record SnapshotHistoryItemDto(
    DateTime Timestamp,
    int ICUAvailable,
    int ICUTotal,
    int GeneralAvailable,
    int GeneralTotal,
    int EmergencyAvailable,
    int OTAvailable
);

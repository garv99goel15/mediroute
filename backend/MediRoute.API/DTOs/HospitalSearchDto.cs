using MediRoute.API.Models;

namespace MediRoute.API.DTOs;

public record HospitalSearchDto(
    double Lat,
    double Lng,
    string? Service,
    string? Specialization,
    double Radius = 10
);

public record HospitalSearchResultDto(
    int Id,
    string Name,
    string Address,
    string City,
    double Lat,
    double Lng,
    double DistanceKm,
    double Score,
    AvailabilityDto Availability
);

public record DepartmentAvailabilityDto(
    int DepartmentId,
    string Name,
    DepartmentType Type,
    int Available,
    int Total,
    string ColorCode
);

public record AvailabilityDto(
    int HospitalId,
    DateTime Timestamp,
    int ICUAvailable,
    int ICUTotal,
    int GeneralAvailable,
    int GeneralTotal,
    int EmergencyAvailable,
    int EmergencyTotal,
    int OTAvailable,
    int OTTotal,
    int MaternityAvailable,
    int MaternityTotal,
    int DoctorsAvailable,
    int DoctorsTotal,
    int AmbulancesAvailable,
    int AmbulancesTotal,
    List<DepartmentAvailabilityDto> Departments
);

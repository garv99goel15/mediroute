namespace MediRoute.API.Models;

/// <summary>Type of department within a hospital.</summary>
public enum DepartmentType
{
    ICU,
    General,
    Emergency,
    OT,
    Maternity
}

/// <summary>Operational status of an individual bed.</summary>
public enum BedStatus
{
    Available,
    Occupied,
    Maintenance,
    Reserved
}

/// <summary>Operational status of an ambulance unit.</summary>
public enum AmbulanceStatus
{
    Available,
    Dispatched,
    Maintenance
}

/// <summary>Role of an authenticated user.</summary>
public enum UserRole
{
    SuperAdmin,
    HospitalAdmin
}

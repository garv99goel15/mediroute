using MediRoute.API.Models;

namespace MediRoute.API.DTOs;

public record BedStatusUpdateDto(BedStatus Status);

public record BulkBedUpdateItem(int BedId, BedStatus Status);

public record BulkBedUpdateDto(List<BulkBedUpdateItem> Updates);

public record DoctorAvailabilityUpdateDto(bool IsAvailable, DateTime? NextAvailableAt);

public record AmbulanceStatusUpdateDto(AmbulanceStatus Status);

public record OtStatusUpdateDto(bool IsActive, int? AvailableRooms);

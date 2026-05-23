using MediRoute.API.DTOs;
using MediRoute.API.Hubs;
using MediRoute.API.Repositories;
using MediRoute.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediRoute.API.Controllers;

[Route("api/doctors")]
[Authorize(Roles = "HospitalAdmin,SuperAdmin")]
public class DoctorsController : ApiControllerBase
{
    private readonly IDoctorRepository _doctors;
    private readonly IAvailabilityService _availability;
    private readonly IResourceBroadcaster _broadcaster;
    private readonly IActivityFeed _activity;

    public DoctorsController(IDoctorRepository doctors, IAvailabilityService availability,
        IResourceBroadcaster broadcaster, IActivityFeed activity)
    {
        _doctors = doctors; _availability = availability;
        _broadcaster = broadcaster; _activity = activity;
    }

    /// <summary>Toggle a doctor's availability and optional next-available timestamp.</summary>
    [HttpPut("{id:int}/availability")]
    public async Task<IActionResult> UpdateAvailability(int id, [FromBody] DoctorAvailabilityUpdateDto dto, CancellationToken ct)
    {
        var doc = await _doctors.GetByIdAsync(id, ct);
        if (doc is null) return NotFound();
        if (!IsSuperAdmin() && CurrentHospitalId() != doc.HospitalId) return Forbid();

        doc.IsAvailable = dto.IsAvailable;
        doc.NextAvailableAt = dto.NextAvailableAt;
        await _doctors.UpdateAsync(doc, ct);

        await _availability.InvalidateAsync(doc.HospitalId);
        var avail = await _availability.GetAvailabilityAsync(doc.HospitalId, ct);
        if (avail.IsSuccess && avail.Value is not null)
            await _broadcaster.BroadcastAsync(doc.HospitalId, "Doctor", avail.Value, ct);

        _activity.Record(doc.HospitalId, "Doctor",
            $"{doc.Name} → {(dto.IsAvailable ? "Available" : "Unavailable")}", CurrentUsername());
        return NoContent();
    }
}

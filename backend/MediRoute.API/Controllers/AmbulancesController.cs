using MediRoute.API.DTOs;
using MediRoute.API.Hubs;
using MediRoute.API.Repositories;
using MediRoute.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediRoute.API.Controllers;

[Route("api/ambulances")]
[Authorize(Roles = "HospitalAdmin,SuperAdmin")]
public class AmbulancesController : ApiControllerBase
{
    private readonly IAmbulanceRepository _repo;
    private readonly IAvailabilityService _availability;
    private readonly IResourceBroadcaster _broadcaster;
    private readonly IActivityFeed _activity;

    public AmbulancesController(IAmbulanceRepository repo, IAvailabilityService availability,
        IResourceBroadcaster broadcaster, IActivityFeed activity)
    {
        _repo = repo; _availability = availability; _broadcaster = broadcaster; _activity = activity;
    }

    /// <summary>Update an ambulance's operational status (Available/Dispatched/Maintenance).</summary>
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] AmbulanceStatusUpdateDto dto, CancellationToken ct)
    {
        var amb = await _repo.GetByIdAsync(id, ct);
        if (amb is null) return NotFound();
        if (!IsSuperAdmin() && CurrentHospitalId() != amb.HospitalId) return Forbid();

        amb.Status = dto.Status;
        amb.LastUpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(amb, ct);

        await _availability.InvalidateAsync(amb.HospitalId);
        var avail = await _availability.GetAvailabilityAsync(amb.HospitalId, ct);
        if (avail.IsSuccess && avail.Value is not null)
            await _broadcaster.BroadcastAsync(amb.HospitalId, "Ambulance", avail.Value, ct);

        _activity.Record(amb.HospitalId, "Ambulance",
            $"{amb.VehicleNumber} → {dto.Status}", CurrentUsername());
        return NoContent();
    }
}

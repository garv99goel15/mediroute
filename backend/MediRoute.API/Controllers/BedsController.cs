using MediRoute.API.DTOs;
using MediRoute.API.Hubs;
using MediRoute.API.Models;
using MediRoute.API.Repositories;
using MediRoute.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediRoute.API.Controllers;

[Route("api/beds")]
[Authorize(Roles = "HospitalAdmin,SuperAdmin")]
public class BedsController : ApiControllerBase
{
    private readonly IBedRepository _beds;
    private readonly IHospitalRepository _hospitals;
    private readonly IAvailabilityService _availability;
    private readonly IResourceBroadcaster _broadcaster;
    private readonly IActivityFeed _activity;
    private readonly ILogger<BedsController> _logger;

    public BedsController(IBedRepository beds, IHospitalRepository hospitals,
        IAvailabilityService availability, IResourceBroadcaster broadcaster,
        IActivityFeed activity, ILogger<BedsController> logger)
    {
        _beds = beds; _hospitals = hospitals; _availability = availability;
        _broadcaster = broadcaster; _activity = activity; _logger = logger;
    }

    /// <summary>Update a single bed's status. HospitalAdmin can only update beds in their own hospital.</summary>
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] BedStatusUpdateDto dto, CancellationToken ct)
    {
        var bed = await _beds.GetByIdAsync(id, ct);
        if (bed is null) return NotFound();
        var hospitalId = bed.Department!.HospitalId;
        if (!await CanModifyAsync(hospitalId)) return Forbid();

        bed.Status = dto.Status;
        bed.LastUpdatedAt = DateTime.UtcNow;
        bed.LastUpdatedBy = CurrentUsername();
        await _beds.UpdateAsync(bed, ct);

        await BroadcastAsync(hospitalId, $"Bed {bed.BedNumber} → {dto.Status}", ct);
        return NoContent();
    }

    /// <summary>Bulk update multiple beds at once. All beds must belong to the caller's hospital.</summary>
    [HttpPut("bulk-update")]
    public async Task<IActionResult> BulkUpdate([FromBody] BulkBedUpdateDto dto, CancellationToken ct)
    {
        var ids = dto.Updates.Select(u => u.BedId).ToList();
        var beds = await _beds.GetByIdsAsync(ids, ct);
        if (beds.Count == 0) return NotFound();

        var hospitalIds = beds.Select(b => b.Department!.HospitalId).Distinct().ToList();
        foreach (var hid in hospitalIds)
            if (!await CanModifyAsync(hid)) return Forbid();

        var map = dto.Updates.ToDictionary(u => u.BedId, u => u.Status);
        var now = DateTime.UtcNow;
        var user = CurrentUsername();
        foreach (var b in beds)
        {
            if (map.TryGetValue(b.Id, out var s))
            {
                b.Status = s;
                b.LastUpdatedAt = now;
                b.LastUpdatedBy = user;
            }
        }
        await _beds.UpdateRangeAsync(beds, ct);

        foreach (var hid in hospitalIds)
            await BroadcastAsync(hid, $"Bulk bed update ({beds.Count(b => b.Department!.HospitalId == hid)} beds)", ct);
        return NoContent();
    }

    private async Task<bool> CanModifyAsync(int hospitalId)
    {
        await Task.CompletedTask;
        if (IsSuperAdmin()) return true;
        var mine = CurrentHospitalId();
        return mine.HasValue && mine.Value == hospitalId;
    }

    private async Task BroadcastAsync(int hospitalId, string description, CancellationToken ct)
    {
        await _availability.InvalidateAsync(hospitalId);
        var avail = await _availability.GetAvailabilityAsync(hospitalId, ct);
        if (avail.IsSuccess && avail.Value is not null)
        {
            await _broadcaster.BroadcastAsync(hospitalId, "Bed", avail.Value, ct);
            _activity.Record(hospitalId, "Bed", description, CurrentUsername());
            _logger.LogInformation("Broadcasted bed update for hospital {Id}: {Desc}", hospitalId, description);
        }
    }
}

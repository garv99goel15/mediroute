using MediRoute.API.DTOs;
using MediRoute.API.Hubs;
using MediRoute.API.Models;
using MediRoute.API.Repositories;
using MediRoute.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediRoute.API.Controllers;

[Route("api")]
[Authorize(Roles = "HospitalAdmin,SuperAdmin")]
public class AdminController : ApiControllerBase
{
    private readonly IDepartmentRepository _departments;
    private readonly IHospitalRepository _hospitals;
    private readonly IAvailabilityService _availability;
    private readonly IResourceBroadcaster _broadcaster;
    private readonly IActivityFeed _activity;
    private readonly ISnapshotRepository _snapshots;

    public AdminController(IDepartmentRepository departments, IHospitalRepository hospitals,
        IAvailabilityService availability, IResourceBroadcaster broadcaster,
        IActivityFeed activity, ISnapshotRepository snapshots)
    {
        _departments = departments; _hospitals = hospitals;
        _availability = availability; _broadcaster = broadcaster;
        _activity = activity; _snapshots = snapshots;
    }

    /// <summary>Mark an OT department active/inactive.</summary>
    [HttpPut("ot/{departmentId:int}/status")]
    public async Task<IActionResult> UpdateOtStatus(int departmentId, [FromBody] OtStatusUpdateDto dto, CancellationToken ct)
    {
        var dept = await _departments.GetByIdAsync(departmentId, ct);
        if (dept is null || dept.Type != DepartmentType.OT) return NotFound();
        if (!IsSuperAdmin() && CurrentHospitalId() != dept.HospitalId) return Forbid();

        dept.IsActive = dto.IsActive;
        await _departments.UpdateAsync(dept, ct);

        await _availability.InvalidateAsync(dept.HospitalId);
        var avail = await _availability.GetAvailabilityAsync(dept.HospitalId, ct);
        if (avail.IsSuccess && avail.Value is not null)
            await _broadcaster.BroadcastAsync(dept.HospitalId, "OT", avail.Value, ct);

        _activity.Record(dept.HospitalId, "OT",
            $"{dept.Name} → {(dto.IsActive ? "Active" : "Inactive")}", CurrentUsername());
        return NoContent();
    }

    /// <summary>Get a full dashboard summary for the current admin's hospital (or any if SuperAdmin).</summary>
    [HttpGet("admin/dashboard")]
    public async Task<ActionResult<AdminDashboardDto>> Dashboard([FromQuery] int? hospitalId, CancellationToken ct)
    {
        var hid = IsSuperAdmin() ? (hospitalId ?? CurrentHospitalId()) : CurrentHospitalId();
        if (hid is null) return BadRequest(new { error = "hospitalId required" });

        var h = await _hospitals.GetByIdWithResourcesAsync(hid.Value, ct);
        if (h is null) return NotFound();

        var avail = _availability.BuildAvailability(h);

        var totalBeds = h.Departments.Sum(d => d.Beds.Count);
        var occupied = h.Departments.SelectMany(d => d.Beds).Count(b => b.Status == BedStatus.Occupied);
        var maintenance = h.Departments.SelectMany(d => d.Beds).Count(b => b.Status == BedStatus.Maintenance);
        var availBeds = h.Departments.SelectMany(d => d.Beds).Count(b => b.Status == BedStatus.Available);

        return Ok(new AdminDashboardDto(
            h.Id, h.Name, totalBeds, availBeds, occupied, maintenance,
            h.Doctors.Count, h.Doctors.Count(d => d.IsAvailable),
            h.Ambulances.Count, h.Ambulances.Count(a => a.Status == AmbulanceStatus.Available),
            avail, _activity.GetRecent(h.Id).ToList()));
    }

    /// <summary>Persist a resource snapshot for analytics/history.</summary>
    [HttpPost("admin/snapshot")]
    public async Task<IActionResult> Snapshot([FromQuery] int? hospitalId, CancellationToken ct)
    {
        var hid = IsSuperAdmin() ? (hospitalId ?? CurrentHospitalId()) : CurrentHospitalId();
        if (hid is null) return BadRequest(new { error = "hospitalId required" });

        var h = await _hospitals.GetByIdWithResourcesAsync(hid.Value, ct);
        if (h is null) return NotFound();
        var a = _availability.BuildAvailability(h);

        await _snapshots.AddAsync(new ResourceSnapshot
        {
            HospitalId = h.Id,
            Timestamp = DateTime.UtcNow,
            ICUAvailable = a.ICUAvailable,
            ICUTotal = a.ICUTotal,
            GeneralAvailable = a.GeneralAvailable,
            GeneralTotal = a.GeneralTotal,
            EmergencyAvailable = a.EmergencyAvailable,
            OTAvailable = a.OTAvailable
        }, ct);

        return Ok(new { message = "Snapshot saved", timestamp = DateTime.UtcNow });
    }
}

using MediRoute.API.DTOs;
using MediRoute.API.Repositories;
using MediRoute.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MediRoute.API.Controllers;

[Route("api/hospitals")]
public class HospitalsController : ApiControllerBase
{
    private readonly IHospitalRepository _repo;
    private readonly IHospitalSearchService _search;
    private readonly IAvailabilityService _availability;
    private readonly ISnapshotRepository _snapshots;

    public HospitalsController(IHospitalRepository repo, IHospitalSearchService search,
        IAvailabilityService availability, ISnapshotRepository snapshots)
    {
        _repo = repo;
        _search = search;
        _availability = availability;
        _snapshots = snapshots;
    }

    /// <summary>Search hospitals by location and optional service/specialization filters.</summary>
    [HttpGet("search")]
    [EnableRateLimiting("search")]
    public async Task<ActionResult<List<HospitalSearchResultDto>>> Search(
        [FromQuery] double lat, [FromQuery] double lng,
        [FromQuery] string? service, [FromQuery] string? specialization,
        [FromQuery] double radius = 10, CancellationToken ct = default)
    {
        var result = await _search.SearchAsync(
            new HospitalSearchDto(lat, lng, service, specialization, radius), ct);
        return ToActionResult(result);
    }

    /// <summary>Get a single hospital with full resource details.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var h = await _repo.GetByIdWithResourcesAsync(id, ct);
        if (h is null) return NotFound();
        return Ok(new
        {
            h.Id, h.Name, h.Address, h.City, h.State,
            h.Lat, h.Long, h.Phone, h.Email, h.IsActive,
            Departments = h.Departments.Select(d => new
            {
                d.Id, d.Name, d.Type, d.TotalBeds, d.IsActive,
                Beds = d.Beds.Select(b => new { b.Id, b.BedNumber, b.Status, b.LastUpdatedAt })
            }),
            Doctors = h.Doctors.Select(d => new { d.Id, d.Name, d.Specialization, d.IsAvailable, d.NextAvailableAt, d.ConsultationFee, d.YearsOfExperience }),
            Ambulances = h.Ambulances.Select(a => new { a.Id, a.VehicleNumber, a.Status, a.LastUpdatedAt })
        });
    }

    /// <summary>Get current availability summary for a hospital (cached 60s).</summary>
    [HttpGet("{id:int}/availability")]
    public async Task<ActionResult<AvailabilityDto>> Availability(int id, CancellationToken ct) =>
        ToActionResult(await _availability.GetAvailabilityAsync(id, ct));

    /// <summary>List doctors for a hospital, optionally filtered by specialization.</summary>
    [HttpGet("{id:int}/doctors")]
    public async Task<IActionResult> Doctors(int id, [FromQuery] string? specialization, CancellationToken ct)
    {
        var h = await _repo.GetByIdWithResourcesAsync(id, ct);
        if (h is null) return NotFound();
        var docs = h.Doctors.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(specialization))
            docs = docs.Where(d => d.Specialization.Equals(specialization, StringComparison.OrdinalIgnoreCase));
        return Ok(docs.Select(d => new { d.Id, d.Name, d.Specialization, d.IsAvailable, d.NextAvailableAt, d.ConsultationFee, d.YearsOfExperience }));
    }

    /// <summary>List hospitals within a radius (km) of a coordinate.</summary>
    [HttpGet("nearby")]
    public async Task<IActionResult> Nearby([FromQuery] double lat, [FromQuery] double lng,
        [FromQuery] double radius = 5, CancellationToken ct = default)
    {
        var list = await _repo.GetNearbyAsync(lat, lng, radius, ct);
        var output = list.Select(h => new
        {
            h.Id, h.Name, h.Address, h.City, h.Lat, h.Long, h.Phone,
            DistanceKm = Math.Round(RoutingService.HaversineKm(lat, lng, h.Lat, h.Long), 2)
        }).Where(x => x.DistanceKm <= radius).OrderBy(x => x.DistanceKm);
        return Ok(output);
    }

    /// <summary>Get snapshot history for a hospital over the last N hours (default 24).</summary>
    [HttpGet("{id:int}/snapshot-history")]
    public async Task<ActionResult<List<SnapshotHistoryItemDto>>> SnapshotHistory(int id,
        [FromQuery] int hours = 24, CancellationToken ct = default)
    {
        var since = DateTime.UtcNow.AddHours(-Math.Abs(hours));
        var rows = await _snapshots.GetForHospitalSinceAsync(id, since, ct);
        return Ok(rows.Select(s => new SnapshotHistoryItemDto(
            s.Timestamp, s.ICUAvailable, s.ICUTotal, s.GeneralAvailable, s.GeneralTotal,
            s.EmergencyAvailable, s.OTAvailable)));
    }
}

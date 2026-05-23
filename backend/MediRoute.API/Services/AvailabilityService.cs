using MediRoute.API.Common;
using MediRoute.API.DTOs;
using MediRoute.API.Models;
using MediRoute.API.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace MediRoute.API.Services;

public interface IAvailabilityService
{
    Task<Result<AvailabilityDto>> GetAvailabilityAsync(int hospitalId, CancellationToken ct = default);
    Task InvalidateAsync(int hospitalId);
    AvailabilityDto BuildAvailability(Hospital hospital);
}

/// <summary>
/// Computes per-hospital availability with a 60s in-memory cache.
/// Cache key is per hospitalId; invalidated on any admin mutation.
/// </summary>
public class AvailabilityService : IAvailabilityService
{
    private readonly IHospitalRepository _repo;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AvailabilityService> _logger;
    private readonly int _ttlSeconds;

    public AvailabilityService(IHospitalRepository repo, IMemoryCache cache,
        ILogger<AvailabilityService> logger, IConfiguration config)
    {
        _repo = repo;
        _cache = cache;
        _logger = logger;
        _ttlSeconds = config.GetValue<int?>("Cache:AvailabilityTTLSeconds") ?? 60;
    }

    private static string Key(int hospitalId) => $"availability:{hospitalId}";

    public async Task<Result<AvailabilityDto>> GetAvailabilityAsync(int hospitalId, CancellationToken ct = default)
    {
        if (_cache.TryGetValue<AvailabilityDto>(Key(hospitalId), out var cached) && cached is not null)
            return Result<AvailabilityDto>.Success(cached);

        var hospital = await _repo.GetByIdWithResourcesAsync(hospitalId, ct);
        if (hospital is null)
            return Result<AvailabilityDto>.Failure("Hospital not found", 404);

        var dto = BuildAvailability(hospital);
        _cache.Set(Key(hospitalId), dto, TimeSpan.FromSeconds(_ttlSeconds));
        return Result<AvailabilityDto>.Success(dto);
    }

    public Task InvalidateAsync(int hospitalId)
    {
        _cache.Remove(Key(hospitalId));
        _logger.LogDebug("Cache invalidated for hospital {Id}", hospitalId);
        return Task.CompletedTask;
    }

    public AvailabilityDto BuildAvailability(Hospital h)
    {
        int icuA = 0, icuT = 0, genA = 0, genT = 0, emA = 0, emT = 0, otA = 0, otT = 0, matA = 0, matT = 0;
        var deptList = new List<DepartmentAvailabilityDto>();

        foreach (var d in h.Departments.Where(x => x.IsActive))
        {
            var avail = d.Beds.Count(b => b.Status == BedStatus.Available);
            var total = d.TotalBeds == 0 ? d.Beds.Count : d.TotalBeds;
            deptList.Add(new DepartmentAvailabilityDto(d.Id, d.Name, d.Type, avail, total, ColorFor(avail, total)));

            switch (d.Type)
            {
                case DepartmentType.ICU: icuA += avail; icuT += total; break;
                case DepartmentType.General: genA += avail; genT += total; break;
                case DepartmentType.Emergency: emA += avail; emT += total; break;
                case DepartmentType.OT: otA += avail; otT += total; break;
                case DepartmentType.Maternity: matA += avail; matT += total; break;
            }
        }

        var docsT = h.Doctors.Count;
        var docsA = h.Doctors.Count(d => d.IsAvailable);
        var ambT = h.Ambulances.Count;
        var ambA = h.Ambulances.Count(a => a.Status == AmbulanceStatus.Available);

        return new AvailabilityDto(
            h.Id, DateTime.UtcNow,
            icuA, icuT, genA, genT, emA, emT, otA, otT, matA, matT,
            docsA, docsT, ambA, ambT, deptList
        );
    }

    /// <summary>Color coding: Green &gt; 50%, Yellow 20–50%, Red &lt; 20%, Grey if total == 0.</summary>
    public static string ColorFor(int available, int total)
    {
        if (total <= 0) return "grey";
        var ratio = (double)available / total;
        if (ratio > 0.5) return "green";
        if (ratio >= 0.2) return "yellow";
        return "red";
    }
}

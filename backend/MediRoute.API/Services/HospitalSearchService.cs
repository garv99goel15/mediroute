using MediRoute.API.Common;
using MediRoute.API.DTOs;
using MediRoute.API.Models;
using MediRoute.API.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace MediRoute.API.Services;

public interface IHospitalSearchService
{
    Task<Result<List<HospitalSearchResultDto>>> SearchAsync(HospitalSearchDto query, CancellationToken ct = default);
}

/// <summary>
/// Ranks hospitals by combined Availability (60%) and Proximity (40%) score.
/// Filters out hospitals where the requested service has 0 availability.
/// Results are cached briefly (default 30s) by query parameters so identical
/// patient searches in burst traffic don't re-hit the DB; SignalR pushes deltas live.
/// </summary>
public class HospitalSearchService : IHospitalSearchService
{
    private readonly IHospitalRepository _repo;
    private readonly IAvailabilityService _availability;
    private readonly IMemoryCache _cache;
    private readonly ILogger<HospitalSearchService> _logger;
    private readonly TimeSpan _cacheTtl;

    public HospitalSearchService(IHospitalRepository repo, IAvailabilityService availability,
        IMemoryCache cache, IConfiguration config, ILogger<HospitalSearchService> logger)
    {
        _repo = repo;
        _availability = availability;
        _cache = cache;
        _logger = logger;
        _cacheTtl = TimeSpan.FromSeconds(Math.Max(0, config.GetValue<int?>("Cache:SearchTTLSeconds") ?? 30));
    }

    public async Task<Result<List<HospitalSearchResultDto>>> SearchAsync(HospitalSearchDto q, CancellationToken ct = default)
    {
        if (q.Radius <= 0)
            return Result<List<HospitalSearchResultDto>>.Failure("Radius must be > 0", 400);

        // Quantize coords to ~110 m grid (4 decimals) so nearby clients share cache entries.
        var cacheKey = $"search:{Math.Round(q.Lat, 4)}:{Math.Round(q.Lng, 4)}:{q.Radius}:{q.Service}:{q.Specialization}";
        if (_cacheTtl > TimeSpan.Zero
            && _cache.TryGetValue<List<HospitalSearchResultDto>>(cacheKey, out var cached)
            && cached is not null)
        {
            return Result<List<HospitalSearchResultDto>>.Success(cached);
        }

        var results = await ComputeSearchAsync(q, ct);

        if (_cacheTtl > TimeSpan.Zero)
        {
            _cache.Set(cacheKey, results, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheTtl,
                Size = 1,
            });
        }
        return Result<List<HospitalSearchResultDto>>.Success(results);
    }

    private async Task<List<HospitalSearchResultDto>> ComputeSearchAsync(HospitalSearchDto q, CancellationToken ct)
    {
        var maxRadius = q.Radius;
        var candidates = await _repo.GetNearbyAsync(q.Lat, q.Lng, maxRadius, ct);

        var results = new List<HospitalSearchResultDto>();
        foreach (var h in candidates)
        {
            var distance = RoutingService.HaversineKm(q.Lat, q.Lng, h.Lat, h.Long);
            if (distance > maxRadius) continue;

            var availability = _availability.BuildAvailability(h);

            var (available, total) = GetAvailabilityForService(availability, h, q.Service, q.Specialization);

            // Hard filter: if service requested and 0 available, skip.
            if (!string.IsNullOrWhiteSpace(q.Service) && available == 0) continue;

            var availScore = total > 0 ? (double)available / total : 0;
            var proxScore = Math.Max(0, 1 - (distance / maxRadius));
            var score = (availScore * 0.6) + (proxScore * 0.4);

            results.Add(new HospitalSearchResultDto(
                h.Id, h.Name, h.Address, h.City, h.Lat, h.Long,
                Math.Round(distance, 2), Math.Round(score, 4), availability));
        }

        return results.OrderByDescending(r => r.Score).Take(20).ToList();
    }

    private static (int Available, int Total) GetAvailabilityForService(
        AvailabilityDto a, Hospital h, string? service, string? specialization)
    {
        if (string.IsNullOrWhiteSpace(service))
        {
            // Default: total beds across departments.
            var totalAvail = a.ICUAvailable + a.GeneralAvailable + a.EmergencyAvailable + a.OTAvailable + a.MaternityAvailable;
            var totalAll = a.ICUTotal + a.GeneralTotal + a.EmergencyTotal + a.OTTotal + a.MaternityTotal;
            return (totalAvail, Math.Max(1, totalAll));
        }

        return service.ToLowerInvariant() switch
        {
            "icu" => (a.ICUAvailable, Math.Max(1, a.ICUTotal)),
            "general" => (a.GeneralAvailable, Math.Max(1, a.GeneralTotal)),
            "emergency" => (a.EmergencyAvailable, Math.Max(1, a.EmergencyTotal)),
            "ot" => (a.OTAvailable, Math.Max(1, a.OTTotal)),
            "maternity" => (a.MaternityAvailable, Math.Max(1, a.MaternityTotal)),
            "doctor" => DoctorAvailability(h, specialization),
            _ => (0, 1)
        };
    }

    private static (int, int) DoctorAvailability(Hospital h, string? specialization)
    {
        var docs = string.IsNullOrWhiteSpace(specialization)
            ? h.Doctors
            : h.Doctors.Where(d => d.Specialization.Equals(specialization, StringComparison.OrdinalIgnoreCase));
        var list = docs.ToList();
        return (list.Count(d => d.IsAvailable), Math.Max(1, list.Count));
    }
}

using MediRoute.API.Data;
using MediRoute.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MediRoute.API.Repositories;

public interface IHospitalRepository
{
    Task<List<Hospital>> GetAllActiveWithResourcesAsync(CancellationToken ct = default);
    Task<Hospital?> GetByIdWithResourcesAsync(int id, CancellationToken ct = default);
    Task<List<Hospital>> GetNearbyAsync(double lat, double lng, double radiusKm, CancellationToken ct = default);
}

public class HospitalRepository : IHospitalRepository
{
    private readonly AppDbContext _db;
    public HospitalRepository(AppDbContext db) { _db = db; }

    public Task<List<Hospital>> GetAllActiveWithResourcesAsync(CancellationToken ct = default) =>
        _db.Hospitals.AsNoTracking()
            .Where(h => h.IsActive)
            .Include(h => h.Departments).ThenInclude(d => d.Beds)
            .Include(h => h.Doctors)
            .Include(h => h.Ambulances)
            .ToListAsync(ct);

    public Task<Hospital?> GetByIdWithResourcesAsync(int id, CancellationToken ct = default) =>
        _db.Hospitals.AsNoTracking()
            .Include(h => h.Departments).ThenInclude(d => d.Beds)
            .Include(h => h.Doctors)
            .Include(h => h.Ambulances)
            .FirstOrDefaultAsync(h => h.Id == id, ct);

    public Task<List<Hospital>> GetNearbyAsync(double lat, double lng, double radiusKm, CancellationToken ct = default)
    {
        // Rough bounding box pre-filter (1 deg lat ~ 111 km).
        var dLat = radiusKm / 111.0;
        var dLng = radiusKm / (111.0 * Math.Cos(lat * Math.PI / 180.0));
        return _db.Hospitals.AsNoTracking()
            .Where(h => h.IsActive
                && h.Lat >= lat - dLat && h.Lat <= lat + dLat
                && h.Long >= lng - dLng && h.Long <= lng + dLng)
            .Include(h => h.Departments).ThenInclude(d => d.Beds)
            .Include(h => h.Doctors)
            .Include(h => h.Ambulances)
            .ToListAsync(ct);
    }
}

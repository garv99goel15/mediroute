using MediRoute.API.Data;
using MediRoute.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MediRoute.API.Repositories;

public interface IBedRepository
{
    Task<Bed?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Bed>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    Task UpdateAsync(Bed bed, CancellationToken ct = default);
    Task UpdateRangeAsync(IEnumerable<Bed> beds, CancellationToken ct = default);
    Task<int> GetHospitalIdForBedAsync(int bedId, CancellationToken ct = default);
}

public class BedRepository : IBedRepository
{
    private readonly AppDbContext _db;
    public BedRepository(AppDbContext db) { _db = db; }

    public Task<Bed?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Beds.Include(b => b.Department).FirstOrDefaultAsync(b => b.Id == id, ct);

    public Task<List<Bed>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default) =>
        _db.Beds.Include(b => b.Department).Where(b => ids.Contains(b.Id)).ToListAsync(ct);

    public async Task UpdateAsync(Bed bed, CancellationToken ct = default)
    {
        _db.Beds.Update(bed);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateRangeAsync(IEnumerable<Bed> beds, CancellationToken ct = default)
    {
        _db.Beds.UpdateRange(beds);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<int> GetHospitalIdForBedAsync(int bedId, CancellationToken ct = default)
    {
        return await _db.Beds.AsNoTracking()
            .Where(b => b.Id == bedId)
            .Select(b => b.Department!.HospitalId)
            .FirstOrDefaultAsync(ct);
    }
}

public interface IDoctorRepository
{
    Task<Doctor?> GetByIdAsync(int id, CancellationToken ct = default);
    Task UpdateAsync(Doctor doctor, CancellationToken ct = default);
}

public class DoctorRepository : IDoctorRepository
{
    private readonly AppDbContext _db;
    public DoctorRepository(AppDbContext db) { _db = db; }

    public Task<Doctor?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Doctors.FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task UpdateAsync(Doctor doctor, CancellationToken ct = default)
    {
        _db.Doctors.Update(doctor);
        await _db.SaveChangesAsync(ct);
    }
}

public interface IAmbulanceRepository
{
    Task<Ambulance?> GetByIdAsync(int id, CancellationToken ct = default);
    Task UpdateAsync(Ambulance ambulance, CancellationToken ct = default);
}

public class AmbulanceRepository : IAmbulanceRepository
{
    private readonly AppDbContext _db;
    public AmbulanceRepository(AppDbContext db) { _db = db; }

    public Task<Ambulance?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Ambulances.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task UpdateAsync(Ambulance ambulance, CancellationToken ct = default)
    {
        _db.Ambulances.Update(ambulance);
        await _db.SaveChangesAsync(ct);
    }
}

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(int id, CancellationToken ct = default);
    Task UpdateAsync(Department department, CancellationToken ct = default);
}

public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _db;
    public DepartmentRepository(AppDbContext db) { _db = db; }

    public Task<Department?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Departments.FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task UpdateAsync(Department department, CancellationToken ct = default)
    {
        _db.Departments.Update(department);
        await _db.SaveChangesAsync(ct);
    }
}

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<User?> GetByRefreshTokenAsync(string token, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
}

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) { _db = db; }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        _db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

    public Task<User?> GetByRefreshTokenAsync(string token, CancellationToken ct = default) =>
        _db.Users.FirstOrDefaultAsync(u => u.RefreshToken == token, ct);

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync(ct);
    }
}

public interface ISnapshotRepository
{
    Task AddAsync(ResourceSnapshot snapshot, CancellationToken ct = default);
    Task<List<ResourceSnapshot>> GetForHospitalSinceAsync(int hospitalId, DateTime since, CancellationToken ct = default);
}

public class SnapshotRepository : ISnapshotRepository
{
    private readonly AppDbContext _db;
    public SnapshotRepository(AppDbContext db) { _db = db; }

    public async Task AddAsync(ResourceSnapshot snapshot, CancellationToken ct = default)
    {
        _db.ResourceSnapshots.Add(snapshot);
        await _db.SaveChangesAsync(ct);
    }

    public Task<List<ResourceSnapshot>> GetForHospitalSinceAsync(int hospitalId, DateTime since, CancellationToken ct = default) =>
        _db.ResourceSnapshots.AsNoTracking()
            .Where(s => s.HospitalId == hospitalId && s.Timestamp >= since)
            .OrderBy(s => s.Timestamp)
            .ToListAsync(ct);
}

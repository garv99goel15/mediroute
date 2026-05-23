using MediRoute.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MediRoute.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Hospital> Hospitals => Set<Hospital>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Bed> Beds => Set<Bed>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Ambulance> Ambulances => Set<Ambulance>();
    public DbSet<ResourceSnapshot> ResourceSnapshots => Set<ResourceSnapshot>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Hospital>()
            .HasIndex(h => new { h.Lat, h.Long });

        modelBuilder.Entity<Department>()
            .HasOne(d => d.Hospital)
            .WithMany(h => h.Departments)
            .HasForeignKey(d => d.HospitalId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Bed>()
            .HasOne(b => b.Department)
            .WithMany(d => d.Beds)
            .HasForeignKey(b => b.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Doctor>()
            .HasOne(d => d.Hospital)
            .WithMany(h => h.Doctors)
            .HasForeignKey(d => d.HospitalId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Ambulance>()
            .HasOne(a => a.Hospital)
            .WithMany(h => h.Ambulances)
            .HasForeignKey(a => a.HospitalId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ResourceSnapshot>()
            .HasOne(s => s.Hospital)
            .WithMany(h => h.Snapshots)
            .HasForeignKey(s => s.HospitalId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<Doctor>().Property(d => d.ConsultationFee).HasPrecision(10, 2);
    }
}

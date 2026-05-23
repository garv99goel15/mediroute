using MediRoute.API.Models;
using MediRoute.API.Repositories;
using MediRoute.API.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace MediRoute.Tests;

public class AvailabilityServiceTests
{
    private static AvailabilityService Build(IHospitalRepository repo)
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Cache:AvailabilityTTLSeconds"] = "60"
        }).Build();
        return new AvailabilityService(repo, cache, NullLogger<AvailabilityService>.Instance, cfg);
    }

    private static Hospital BuildHospital()
    {
        var icu = new Department { Id = 1, Type = DepartmentType.ICU, Name = "ICU", TotalBeds = 4, IsActive = true };
        icu.Beds = new List<Bed>
        {
            new() { Id = 1, DepartmentId = 1, Status = BedStatus.Available, BedNumber = "A1" },
            new() { Id = 2, DepartmentId = 1, Status = BedStatus.Available, BedNumber = "A2" },
            new() { Id = 3, DepartmentId = 1, Status = BedStatus.Occupied, BedNumber = "A3" },
            new() { Id = 4, DepartmentId = 1, Status = BedStatus.Maintenance, BedNumber = "A4" },
        };
        return new Hospital
        {
            Id = 99, Name = "Test", Address = "x", City = "Delhi", State = "Delhi",
            Lat = 0, Long = 0, IsActive = true,
            Departments = new List<Department> { icu },
            Doctors = new List<Doctor>
            {
                new() { Id = 1, HospitalId = 99, Name = "D1", Specialization = "Cardiology", IsAvailable = true },
                new() { Id = 2, HospitalId = 99, Name = "D2", Specialization = "Neurology", IsAvailable = false },
            },
            Ambulances = new List<Ambulance>
            {
                new() { Id = 1, HospitalId = 99, VehicleNumber = "AMB-1", Status = AmbulanceStatus.Available },
            }
        };
    }

    [Fact]
    public async Task GetAvailability_ReturnsCorrectCounts()
    {
        var repo = new Mock<IHospitalRepository>();
        repo.Setup(r => r.GetByIdWithResourcesAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(BuildHospital());

        var svc = Build(repo.Object);
        var result = await svc.GetAvailabilityAsync(99);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.ICUAvailable);
        Assert.Equal(4, result.Value!.ICUTotal);
        Assert.Equal(1, result.Value!.DoctorsAvailable);
        Assert.Equal(2, result.Value!.DoctorsTotal);
        Assert.Equal(1, result.Value!.AmbulancesAvailable);
    }

    [Fact]
    public async Task GetAvailability_UsesCache_OnSecondCall()
    {
        var repo = new Mock<IHospitalRepository>();
        repo.Setup(r => r.GetByIdWithResourcesAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(BuildHospital());

        var svc = Build(repo.Object);
        await svc.GetAvailabilityAsync(99);
        await svc.GetAvailabilityAsync(99);

        repo.Verify(r => r.GetByIdWithResourcesAsync(99, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Invalidate_ForcesReFetch()
    {
        var repo = new Mock<IHospitalRepository>();
        repo.Setup(r => r.GetByIdWithResourcesAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(BuildHospital());

        var svc = Build(repo.Object);
        await svc.GetAvailabilityAsync(99);
        await svc.InvalidateAsync(99);
        await svc.GetAvailabilityAsync(99);

        repo.Verify(r => r.GetByIdWithResourcesAsync(99, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetAvailability_MissingHospital_ReturnsFailure404()
    {
        var repo = new Mock<IHospitalRepository>();
        repo.Setup(r => r.GetByIdWithResourcesAsync(123, It.IsAny<CancellationToken>())).ReturnsAsync((Hospital?)null);

        var svc = Build(repo.Object);
        var result = await svc.GetAvailabilityAsync(123);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Theory]
    [InlineData(10, 10, "green")]
    [InlineData(6, 10, "green")]
    [InlineData(3, 10, "yellow")]
    [InlineData(1, 10, "red")]
    [InlineData(0, 0, "grey")]
    public void ColorFor_ReturnsExpected(int avail, int total, string expected) =>
        Assert.Equal(expected, AvailabilityService.ColorFor(avail, total));
}

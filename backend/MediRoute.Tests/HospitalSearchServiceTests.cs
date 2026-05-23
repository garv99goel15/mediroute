using MediRoute.API.DTOs;
using MediRoute.API.Models;
using MediRoute.API.Repositories;
using MediRoute.API.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace MediRoute.Tests;

public class HospitalSearchServiceTests
{
    private static IAvailabilityService BuildAvailability()
    {
        var repo = new Mock<IHospitalRepository>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Cache:AvailabilityTTLSeconds"] = "60"
        }).Build();
        return new AvailabilityService(repo.Object, cache, NullLogger<AvailabilityService>.Instance, cfg);
    }

    private static Hospital MakeHospital(int id, double lat, double lng, int icuTotal, int icuAvail)
    {
        var dept = new Department
        {
            Id = id * 10, HospitalId = id, Type = DepartmentType.ICU,
            Name = "ICU", TotalBeds = icuTotal, IsActive = true
        };
        for (var i = 0; i < icuTotal; i++)
        {
            dept.Beds.Add(new Bed
            {
                Id = id * 1000 + i,
                DepartmentId = dept.Id,
                BedNumber = $"ICU-{i}",
                Status = i < icuAvail ? BedStatus.Available : BedStatus.Occupied
            });
        }
        return new Hospital
        {
            Id = id, Name = $"H{id}", Address = "x", City = "Delhi", State = "Delhi",
            Lat = lat, Long = lng, IsActive = true, Departments = new List<Department> { dept }
        };
    }

    [Fact]
    public async Task Search_FiltersOutHospitalsWithZeroAvailabilityForService()
    {
        var h1 = MakeHospital(1, 28.5672, 77.2100, 10, 5); // available
        var h2 = MakeHospital(2, 28.5673, 77.2101, 10, 0); // none available — should be filtered

        var repo = new Mock<IHospitalRepository>();
        repo.Setup(r => r.GetNearbyAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Hospital> { h1, h2 });

        var svc = new HospitalSearchService(repo.Object, BuildAvailability(), NullLogger<HospitalSearchService>.Instance);
        var query = new HospitalSearchDto(28.5672, 77.2100, "ICU", null, 10);

        var result = await svc.SearchAsync(query);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal(1, result.Value![0].Id);
    }

    [Fact]
    public async Task Search_RanksByCombinedScore_HigherAvailabilityWins()
    {
        // Both same distance; H1 has higher availability -> should win.
        var h1 = MakeHospital(1, 28.5672, 77.2100, 10, 9);
        var h2 = MakeHospital(2, 28.5672, 77.2100, 10, 2);

        var repo = new Mock<IHospitalRepository>();
        repo.Setup(r => r.GetNearbyAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Hospital> { h2, h1 });

        var svc = new HospitalSearchService(repo.Object, BuildAvailability(), NullLogger<HospitalSearchService>.Instance);
        var result = await svc.SearchAsync(new HospitalSearchDto(28.5672, 77.2100, "ICU", null, 10));

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value![0].Id);
    }

    [Fact]
    public async Task Search_RejectsInvalidRadius()
    {
        var repo = new Mock<IHospitalRepository>();
        var svc = new HospitalSearchService(repo.Object, BuildAvailability(), NullLogger<HospitalSearchService>.Instance);
        var result = await svc.SearchAsync(new HospitalSearchDto(28.5, 77.2, null, null, 0));
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public void Haversine_KnownDistance_IsAccurate()
    {
        // Distance between AIIMS Delhi (28.5672, 77.2100) and Safdarjung (28.5683, 77.2070).
        var d = RoutingService.HaversineKm(28.5672, 77.2100, 28.5683, 77.2070);
        Assert.InRange(d, 0.2, 0.5);
    }
}

using MediRoute.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MediRoute.API.Data;

/// <summary>Seeds 15 hospitals across Delhi NCR with realistic resource mix.</summary>
public static class SeedData
{
    private static readonly (string Name, string Address, string City, double Lat, double Lng)[] Hospitals = new[]
    {
        ("All India Institute of Medical Sciences", "Sri Aurobindo Marg, Ansari Nagar", "New Delhi", 28.5672, 77.2100),
        ("Safdarjung Hospital", "Ansari Nagar West", "New Delhi", 28.5683, 77.2070),
        ("Fortis Escorts Heart Institute", "Okhla Road", "New Delhi", 28.5562, 77.2829),
        ("Max Super Speciality Hospital Saket", "Press Enclave Road, Saket", "New Delhi", 28.5286, 77.2147),
        ("Apollo Hospital Sarita Vihar", "Mathura Road, Sarita Vihar", "New Delhi", 28.5379, 77.2944),
        ("Sir Ganga Ram Hospital", "Rajinder Nagar", "New Delhi", 28.6391, 77.1908),
        ("BLK-Max Super Speciality", "Pusa Road, Rajendra Place", "New Delhi", 28.6448, 77.1875),
        ("Medanta The Medicity", "Sector 38, Gurugram", "Gurugram", 28.4399, 77.0408),
        ("Fortis Memorial Research Institute", "Sector 44, Gurugram", "Gurugram", 28.4501, 77.0721),
        ("Artemis Hospital", "Sector 51, Gurugram", "Gurugram", 28.4356, 77.0625),
        ("Max Super Speciality Hospital Patparganj", "108A, IP Extension, Patparganj", "New Delhi", 28.6276, 77.2913),
        ("Yashoda Super Speciality Hospital", "H1, Kaushambi", "Ghaziabad", 28.6450, 77.3236),
        ("Jaypee Hospital", "Sector 128, Noida", "Noida", 28.5180, 77.3792),
        ("Fortis Hospital Noida", "B-22, Sector 62", "Noida", 28.6285, 77.3727),
        ("Kailash Hospital", "H-33, Sector 27", "Noida", 28.5734, 77.3260)
    };

    private static readonly string[] Specializations = new[]
    {
        "Cardiology", "Neurology", "Orthopedics", "Pediatrics", "Oncology",
        "General Medicine", "Gynecology", "Dermatology", "Pulmonology", "Nephrology",
        "Gastroenterology", "Urology", "ENT", "Ophthalmology", "Endocrinology"
    };

    public static async Task SeedAsync(AppDbContext db, ILogger logger)
    {
        if (await db.Hospitals.AnyAsync())
        {
            logger.LogInformation("Database already seeded — skipping.");
            return;
        }

        var rng = new Random(42);

        foreach (var (name, address, city, lat, lng) in Hospitals)
        {
            var hospital = new Hospital
            {
                Name = name,
                Address = address,
                City = city,
                State = city == "Gurugram" ? "Haryana" : (city == "Noida" || city == "Ghaziabad" ? "Uttar Pradesh" : "Delhi"),
                Lat = lat,
                Long = lng,
                Phone = $"+91-11-{rng.Next(20000000, 99999999)}",
                Email = $"contact@{name.Split(' ')[0].ToLower()}.in",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // 3–5 departments
            var deptCount = rng.Next(3, 6);
            var allTypes = Enum.GetValues<DepartmentType>().OrderBy(_ => rng.Next()).Take(deptCount).ToList();
            foreach (var type in allTypes)
            {
                var totalBeds = rng.Next(10, 31);
                var dept = new Department
                {
                    Name = $"{type} Department",
                    Type = type,
                    TotalBeds = totalBeds,
                    IsActive = true
                };
                for (var i = 1; i <= totalBeds; i++)
                {
                    var roll = rng.NextDouble();
                    var status = roll switch
                    {
                        < 0.45 => BedStatus.Available,
                        < 0.85 => BedStatus.Occupied,
                        < 0.95 => BedStatus.Reserved,
                        _ => BedStatus.Maintenance
                    };
                    dept.Beds.Add(new Bed
                    {
                        BedNumber = $"{type.ToString()[..Math.Min(3, type.ToString().Length)].ToUpper()}-{i:D3}",
                        Status = status,
                        LastUpdatedAt = DateTime.UtcNow.AddMinutes(-rng.Next(0, 240)),
                        LastUpdatedBy = "system"
                    });
                }
                hospital.Departments.Add(dept);
            }

            // 5–15 doctors
            var docCount = rng.Next(5, 16);
            for (var i = 0; i < docCount; i++)
            {
                hospital.Doctors.Add(new Doctor
                {
                    Name = $"Dr. {GetRandomFirstName(rng)} {GetRandomLastName(rng)}",
                    Specialization = Specializations[rng.Next(Specializations.Length)],
                    IsAvailable = rng.NextDouble() < 0.65,
                    NextAvailableAt = rng.NextDouble() < 0.5 ? DateTime.UtcNow.AddHours(rng.Next(1, 8)) : null,
                    ConsultationFee = rng.Next(500, 3001),
                    YearsOfExperience = rng.Next(3, 35)
                });
            }

            // 2–5 ambulances
            var ambCount = rng.Next(2, 6);
            for (var i = 1; i <= ambCount; i++)
            {
                var roll = rng.NextDouble();
                hospital.Ambulances.Add(new Ambulance
                {
                    VehicleNumber = $"DL-{rng.Next(1, 20):D2}-AB-{rng.Next(1000, 9999)}",
                    Status = roll < 0.65 ? AmbulanceStatus.Available
                           : roll < 0.9 ? AmbulanceStatus.Dispatched
                           : AmbulanceStatus.Maintenance,
                    LastUpdatedAt = DateTime.UtcNow.AddMinutes(-rng.Next(0, 120))
                });
            }

            db.Hospitals.Add(hospital);
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} hospitals.", Hospitals.Length);

        // Seed users
        if (!await db.Users.AnyAsync())
        {
            db.Users.Add(new User
            {
                Username = "superadmin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.SuperAdmin,
                HospitalId = null
            });

            // One HospitalAdmin per hospital: admin1 / Admin@123 -> hospital 1, etc.
            var hospitalIds = await db.Hospitals.AsNoTracking().OrderBy(h => h.Id).Select(h => h.Id).ToListAsync();
            var idx = 1;
            foreach (var hid in hospitalIds)
            {
                db.Users.Add(new User
                {
                    Username = $"admin{idx}",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = UserRole.HospitalAdmin,
                    HospitalId = hid
                });
                idx++;
            }
            await db.SaveChangesAsync();
            logger.LogInformation("Seeded users (superadmin + {Count} hospital admins). Default password: Admin@123", hospitalIds.Count);
        }
    }

    private static readonly string[] FirstNames = { "Arjun", "Priya", "Rahul", "Ananya", "Vikram", "Neha", "Rohan", "Pooja", "Karan", "Sneha", "Aditya", "Ishita", "Manish", "Divya", "Sanjay" };
    private static readonly string[] LastNames = { "Sharma", "Verma", "Gupta", "Singh", "Kumar", "Patel", "Reddy", "Iyer", "Mehta", "Joshi", "Khanna", "Bansal", "Chopra", "Malhotra", "Kapoor" };

    private static string GetRandomFirstName(Random rng) => FirstNames[rng.Next(FirstNames.Length)];
    private static string GetRandomLastName(Random rng) => LastNames[rng.Next(LastNames.Length)];
}

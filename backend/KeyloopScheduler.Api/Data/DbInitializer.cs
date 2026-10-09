
using KeyloopScheduler.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace KeyloopScheduler.Api.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(
        SchedulerDbContext db,
        CancellationToken cancellationToken = default)
    {
        if (await db.Dealerships.AnyAsync(cancellationToken))
        {
            return;
        }

        var dealership = new Dealership
        {
            Name = "Keyloop Demo Service Centre",
            Address = "100 Demo Road",
            TimeZoneId = "UTC"
        };

        var oilService = new ServiceType
        {
            Name = "Oil Change",
            DurationMinutes = 60,
            RequiredSkill = "OilChange",
            IsActive = true
        };

        var brakeService = new ServiceType
        {
            Name = "Brake Service",
            DurationMinutes = 90,
            RequiredSkill = "BrakeService",
            IsActive = true
        };

        var customer = new Customer
        {
            Name = "Alex Morgan",
            Email = "alex.morgan@example.com",
            Phone = "555-0100"
        };

        var vehicle = new Vehicle
        {
            Customer = customer,
            VIN = "DEMO1234567890001",
            Make = "Toyota",
            Model = "Camry",
            Year = 2024
        };

        var technician1 = new Technician
        {
            Dealership = dealership,
            Name = "Jordan Lee",
            IsActive = true,
            Skills =
            {
                new TechnicianSkill { SkillName = "OilChange" },
                new TechnicianSkill { SkillName = "BrakeService" }
            }
        };

        var technician2 = new Technician
        {
            Dealership = dealership,
            Name = "Taylor Smith",
            IsActive = true,
            Skills =
            {
                new TechnicianSkill { SkillName = "OilChange" }
            }
        };

        var bay1 = new ServiceBay
        {
            Dealership = dealership,
            BayName = "Bay 1",
            IsActive = true
        };

        var bay2 = new ServiceBay
        {
            Dealership = dealership,
            BayName = "Bay 2",
            IsActive = true
        };

        db.AddRange(
            dealership,
            oilService,
            brakeService,
            customer,
            vehicle,
            technician1,
            technician2,
            bay1,
            bay2);

        await db.SaveChangesAsync(cancellationToken);
    }
}

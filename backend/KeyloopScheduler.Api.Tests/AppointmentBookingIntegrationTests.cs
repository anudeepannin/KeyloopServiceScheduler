using KeyloopScheduler.Api.Contracts;
using KeyloopScheduler.Api.Controllers;
using KeyloopScheduler.Api.Data;
using KeyloopScheduler.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace KeyloopScheduler.Api.Tests;

public class AppointmentBookingIntegrationTests
{
    [Fact]
    public async Task CreateAppointment_WhenBookingIsValid_ReturnsCreatedAppointment()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<SchedulerDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new SchedulerDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var dealership = new Dealership
        {
            Name = "Test Dealership",
            Address = "Test Address",
            TimeZoneId = "UTC"
        };

        var customer = new Customer
        {
            Name = "Test Customer",
            Email = "test@example.com",
            Phone = "555-0100"
        };

        var service = new ServiceType
        {
            Name = "Oil Change",
            DurationMinutes = 60,
            RequiredSkill = "OilChange",
            IsActive = true
        };

        db.Dealerships.Add(dealership);
        db.Customers.Add(customer);
        db.ServiceTypes.Add(service);
        await db.SaveChangesAsync();

        var vehicle = new Vehicle
        {
            CustomerId = customer.CustomerId,
            Customer = customer,
            VIN = "TEST1234567890001",
            Make = "Toyota",
            Model = "Camry",
            Year = 2024
        };

        var technician = new Technician
        {
            Name = "Test Technician",
            DealershipId = dealership.DealershipId,
            Dealership = dealership,
            IsActive = true
        };

        var bay = new ServiceBay
        {
            BayName = "Bay 1",
            DealershipId = dealership.DealershipId,
            Dealership = dealership,
            IsActive = true
        };

        db.Vehicles.Add(vehicle);
        db.Technicians.Add(technician);
        db.ServiceBays.Add(bay);
        await db.SaveChangesAsync();

        db.TechnicianSkills.Add(new TechnicianSkill
        {
            TechnicianId = technician.TechnicianId,
            Technician = technician,
            SkillName = "OilChange"
        });

        await db.SaveChangesAsync();

        var controller = new AppointmentsController(db);

        var request = new CreateAppointmentRequest
        {
            CustomerId = customer.CustomerId,
            VehicleId = vehicle.VehicleId,
            DealershipId = dealership.DealershipId,
            ServiceTypeId = service.ServiceTypeId,
            TechnicianId = technician.TechnicianId,
            ServiceBayId = bay.ServiceBayId,
            StartTimeUtc = DateTimeOffset.UtcNow.AddDays(2)
        };

        var result = await controller.CreateAppointment(
            request, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.NotNull(created.Value);

        var savedAppointment = await db.Appointments.SingleAsync();

        Assert.Equal("Confirmed", savedAppointment.Status);
        Assert.Equal(customer.CustomerId, savedAppointment.CustomerId);
        Assert.Equal(technician.TechnicianId, savedAppointment.TechnicianId);
        Assert.Equal(bay.ServiceBayId, savedAppointment.ServiceBayId);
        Assert.Equal(60,
            (savedAppointment.EndTimeUtc - savedAppointment.StartTimeUtc).TotalMinutes);
    }
    private static async Task<(SqliteConnection Connection, SchedulerDbContext Db,
    Customer Customer, Vehicle Vehicle, Dealership Dealership,
    ServiceType Service, Technician Technician, ServiceBay Bay)>
    CreateTestDatabaseAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<SchedulerDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new SchedulerDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var dealership = new Dealership
        {
            Name = "Test Dealership",
            Address = "Test Address",
            TimeZoneId = "UTC"
        };

        var customer = new Customer
        {
            Name = "Test Customer",
            Email = $"test-{Guid.NewGuid():N}@example.com",
            Phone = "555-0100"
        };

        var service = new ServiceType
        {
            Name = "Oil Change",
            DurationMinutes = 60,
            RequiredSkill = "OilChange",
            IsActive = true
        };

        db.AddRange(dealership, customer, service);
        await db.SaveChangesAsync();

        var vehicle = new Vehicle
        {
            CustomerId = customer.CustomerId,
            Customer = customer,
            VIN = "TEST" + Guid.NewGuid().ToString("N")[..13],
            Make = "Toyota",
            Model = "Camry",
            Year = 2024
        };

        var technician = new Technician
        {
            Name = "Test Technician",
            DealershipId = dealership.DealershipId,
            Dealership = dealership,
            IsActive = true
        };

        var bay = new ServiceBay
        {
            BayName = "Bay 1",
            DealershipId = dealership.DealershipId,
            Dealership = dealership,
            IsActive = true
        };

        db.AddRange(vehicle, technician, bay);
        await db.SaveChangesAsync();

        db.TechnicianSkills.Add(new TechnicianSkill
        {
            TechnicianId = technician.TechnicianId,
            Technician = technician,
            SkillName = "OilChange"
        });

        await db.SaveChangesAsync();

        return (connection, db, customer, vehicle, dealership,
            service, technician, bay);
    }
    [Fact]
    public async Task CreateAppointment_WhenTechnicianIsAlreadyBooked_ReturnsConflict()
    {
        var setup = await CreateTestDatabaseAsync();
        await using var connection = setup.Connection;
        await using var db = setup.Db;

        var start = DateTime.UtcNow.AddDays(2);
        start = new DateTime(start.Year, start.Month, start.Day, 10, 0, 0,
            DateTimeKind.Utc);

        db.Appointments.Add(new Appointment
        {
            CustomerId = setup.Customer.CustomerId,
            VehicleId = setup.Vehicle.VehicleId,
            DealershipId = setup.Dealership.DealershipId,
            ServiceTypeId = setup.Service.ServiceTypeId,
            TechnicianId = setup.Technician.TechnicianId,
            ServiceBayId = setup.Bay.ServiceBayId,
            StartTimeUtc = start,
            EndTimeUtc = start.AddHours(1),
            CreatedAtUtc = DateTime.UtcNow,
            Status = "Confirmed"
        });

        await db.SaveChangesAsync();

        var controller = new AppointmentsController(db);
        var request = new CreateAppointmentRequest
        {
            CustomerId = setup.Customer.CustomerId,
            VehicleId = setup.Vehicle.VehicleId,
            DealershipId = setup.Dealership.DealershipId,
            ServiceTypeId = setup.Service.ServiceTypeId,
            TechnicianId = setup.Technician.TechnicianId,
            ServiceBayId = setup.Bay.ServiceBayId,
            StartTimeUtc = new DateTimeOffset(start.AddMinutes(30))
        };

        var result = await controller.CreateAppointment(
            request, CancellationToken.None);

        Assert.IsType<ConflictObjectResult>(result);
        Assert.Single(db.Appointments);
    }
    [Fact]
    public async Task CreateAppointment_WhenServiceBayIsAlreadyBooked_ReturnsConflict()
    {
        var setup = await CreateTestDatabaseAsync();
        await using var connection = setup.Connection;
        await using var db = setup.Db;

        var start = DateTime.UtcNow.AddDays(2);
        start = new DateTime(start.Year, start.Month, start.Day, 10, 0, 0,
            DateTimeKind.Utc);

        db.Appointments.Add(new Appointment
        {
            CustomerId = setup.Customer.CustomerId,
            VehicleId = setup.Vehicle.VehicleId,
            DealershipId = setup.Dealership.DealershipId,
            ServiceTypeId = setup.Service.ServiceTypeId,
            TechnicianId = setup.Technician.TechnicianId,
            ServiceBayId = setup.Bay.ServiceBayId,
            StartTimeUtc = start,
            EndTimeUtc = start.AddHours(1),
            CreatedAtUtc = DateTime.UtcNow,
            Status = "Confirmed"
        });

        await db.SaveChangesAsync();

        var secondTechnician = new Technician
        {
            Name = "Second Technician",
            DealershipId = setup.Dealership.DealershipId,
            Dealership = setup.Dealership,
            IsActive = true
        };

        db.Technicians.Add(secondTechnician);
        await db.SaveChangesAsync();

        db.TechnicianSkills.Add(new TechnicianSkill
        {
            TechnicianId = secondTechnician.TechnicianId,
            Technician = secondTechnician,
            SkillName = "OilChange"
        });

        await db.SaveChangesAsync();

        var controller = new AppointmentsController(db);
        var request = new CreateAppointmentRequest
        {
            CustomerId = setup.Customer.CustomerId,
            VehicleId = setup.Vehicle.VehicleId,
            DealershipId = setup.Dealership.DealershipId,
            ServiceTypeId = setup.Service.ServiceTypeId,
            TechnicianId = secondTechnician.TechnicianId,
            ServiceBayId = setup.Bay.ServiceBayId,
            StartTimeUtc = new DateTimeOffset(start.AddMinutes(30))
        };

        var result = await controller.CreateAppointment(
            request, CancellationToken.None);

        Assert.IsType<ConflictObjectResult>(result);
        Assert.Single(db.Appointments);
    }
    [Fact]
    public async Task CreateAppointment_WhenPreviousAppointmentIsCancelled_AllowsNewBooking()
    {
        var setup = await CreateTestDatabaseAsync();
        await using var connection = setup.Connection;
        await using var db = setup.Db;

        var start = DateTime.UtcNow.AddDays(2);
        start = new DateTime(start.Year, start.Month, start.Day, 10, 0, 0,
            DateTimeKind.Utc);

        db.Appointments.Add(new Appointment
        {
            CustomerId = setup.Customer.CustomerId,
            VehicleId = setup.Vehicle.VehicleId,
            DealershipId = setup.Dealership.DealershipId,
            ServiceTypeId = setup.Service.ServiceTypeId,
            TechnicianId = setup.Technician.TechnicianId,
            ServiceBayId = setup.Bay.ServiceBayId,
            StartTimeUtc = start,
            EndTimeUtc = start.AddHours(1),
            CreatedAtUtc = DateTime.UtcNow,
            Status = "Cancelled"
        });

        await db.SaveChangesAsync();

        var controller = new AppointmentsController(db);

        var request = new CreateAppointmentRequest
        {
            CustomerId = setup.Customer.CustomerId,
            VehicleId = setup.Vehicle.VehicleId,
            DealershipId = setup.Dealership.DealershipId,
            ServiceTypeId = setup.Service.ServiceTypeId,
            TechnicianId = setup.Technician.TechnicianId,
            ServiceBayId = setup.Bay.ServiceBayId,
            StartTimeUtc = new DateTimeOffset(start.AddMinutes(30))
        };

        var result = await controller.CreateAppointment(
            request, CancellationToken.None);

        Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(2, await db.Appointments.CountAsync());
        Assert.Equal(1, await db.Appointments.CountAsync(
            a => a.Status == "Confirmed"));
    }
}
using KeyloopScheduler.Api.Controllers;
using KeyloopScheduler.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KeyloopScheduler.Api.Tests;

public class AppointmentsControllerTests
{
    private static SchedulerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SchedulerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings =>
                warnings.Ignore(
                    Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new SchedulerDbContext(options);
    }

    [Fact]
    public async Task CancelAppointment_WhenAppointmentDoesNotExist_ReturnsNotFound()
    {
        await using var db = CreateDbContext();
        var controller = new AppointmentsController(db);

        var result = await controller.CancelAppointment(
            999, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetAppointment_WhenAppointmentDoesNotExist_ReturnsNotFound()
    {
        await using var db = CreateDbContext();
        var controller = new AppointmentsController(db);

        var result = await controller.GetAppointment(
            999, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }
    [Fact]
    public async Task CreateAppointment_WhenStartTimeIsInThePast_ReturnsBadRequest()
    {
        await using var db = CreateDbContext();
        var controller = new AppointmentsController(db);

        var request = new KeyloopScheduler.Api.Contracts.CreateAppointmentRequest
        {
            CustomerId = 1,
            VehicleId = 1,
            DealershipId = 1,
            ServiceTypeId = 1,
            TechnicianId = 1,
            ServiceBayId = 1,
            StartTimeUtc = DateTimeOffset.UtcNow.AddHours(-1)
        };

        var result = await controller.CreateAppointment(
            request, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateAppointment_WhenCustomerDoesNotExist_ReturnsBadRequest()
    {
        await using var db = CreateDbContext();
        var controller = new AppointmentsController(db);

        var request = new KeyloopScheduler.Api.Contracts.CreateAppointmentRequest
        {
            CustomerId = 999,
            VehicleId = 999,
            DealershipId = 1,
            ServiceTypeId = 1,
            TechnicianId = 1,
            ServiceBayId = 1,
            StartTimeUtc = DateTimeOffset.UtcNow.AddDays(2)
        };

        var result = await controller.CreateAppointment(
            request, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }
    [Fact]
    public async Task CancelAppointment_WhenAlreadyCancelled_ReturnsBadRequest()
    {
        await using var db = CreateDbContext();

        db.Appointments.Add(new KeyloopScheduler.Api.Entities.Appointment
        {
            AppointmentId = 100,
            CustomerId = 1,
            VehicleId = 1,
            DealershipId = 1,
            ServiceTypeId = 1,
            TechnicianId = 1,
            ServiceBayId = 1,
            StartTimeUtc = DateTime.UtcNow.AddDays(1),
            EndTimeUtc = DateTime.UtcNow.AddDays(1).AddHours(1),
            CreatedAtUtc = DateTime.UtcNow,
            Status = "Cancelled"
        });

        await db.SaveChangesAsync();

        var controller = new AppointmentsController(db);

        var result = await controller.CancelAppointment(
            100, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CancelAppointment_WhenAppointmentIsConfirmed_ReturnsOkAndUpdatesStatus()
    {
        await using var db = CreateDbContext();

        db.Appointments.Add(new KeyloopScheduler.Api.Entities.Appointment
        {
            AppointmentId = 101,
            CustomerId = 1,
            VehicleId = 1,
            DealershipId = 1,
            ServiceTypeId = 1,
            TechnicianId = 1,
            ServiceBayId = 1,
            StartTimeUtc = DateTime.UtcNow.AddDays(1),
            EndTimeUtc = DateTime.UtcNow.AddDays(1).AddHours(1),
            CreatedAtUtc = DateTime.UtcNow,
            Status = "Confirmed"
        });

        await db.SaveChangesAsync();

        var controller = new AppointmentsController(db);

        var result = await controller.CancelAppointment(
            101, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);

        var appointment = await db.Appointments.FindAsync(101);

        Assert.NotNull(appointment);
        Assert.Equal("Cancelled", appointment.Status);
    }
}

using System.Data;
using KeyloopScheduler.Api.Contracts;
using KeyloopScheduler.Api.Data;
using KeyloopScheduler.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KeyloopScheduler.Api.Controllers;

[ApiController]
[Route("api/appointments")]
public sealed class AppointmentsController : ControllerBase
{
    private readonly SchedulerDbContext _db;

    public AppointmentsController(SchedulerDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAppointment(
        [FromBody] CreateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var startUtc = request.StartTimeUtc.UtcDateTime;

        if (startUtc <= DateTime.UtcNow)
        {
            return BadRequest(new
            {
                message = "The appointment must start in the future."
            });
        }

        await using var transaction =
            await _db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        var customerExists = await _db.Customers
            .AnyAsync(
                c => c.CustomerId == request.CustomerId,
                cancellationToken);

        if (!customerExists)
        {
            return NotFound(new { message = "Customer not found." });
        }

        var vehicleBelongsToCustomer = await _db.Vehicles
            .AnyAsync(
                v => v.VehicleId == request.VehicleId &&
                     v.CustomerId == request.CustomerId,
                cancellationToken);

        if (!vehicleBelongsToCustomer)
        {
            return BadRequest(new
            {
                message = "The vehicle does not belong to the selected customer."
            });
        }

        var service = await _db.ServiceTypes
            .SingleOrDefaultAsync(
                s => s.ServiceTypeId == request.ServiceTypeId &&
                     s.IsActive,
                cancellationToken);

        if (service is null)
        {
            return NotFound(new
            {
                message = "Active service type not found."
            });
        }

        if (service.DurationMinutes <= 0)
        {
            return Problem(
                title: "Invalid service configuration",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        DateTime endUtc;

        try
        {
            endUtc = startUtc.AddMinutes(service.DurationMinutes);
        }
        catch (ArgumentOutOfRangeException)
        {
            return BadRequest(new
            {
                message = "The requested appointment time is out of range."
            });
        }

        var technician = await _db.Technicians
            .Include(t => t.Skills)
            .SingleOrDefaultAsync(
                t => t.TechnicianId == request.TechnicianId &&
                     t.DealershipId == request.DealershipId &&
                     t.IsActive,
                cancellationToken);

        if (technician is null)
        {
            return BadRequest(new
            {
                message = "The selected technician is unavailable at this dealership."
            });
        }

        var isQualified = technician.Skills.Any(
            s => s.SkillName == service.RequiredSkill);

        if (!isQualified)
        {
            return BadRequest(new
            {
                message = "The technician does not have the required qualification."
            });
        }

        var bayIsValid = await _db.ServiceBays
            .AnyAsync(
                b => b.ServiceBayId == request.ServiceBayId &&
                     b.DealershipId == request.DealershipId &&
                     b.IsActive,
                cancellationToken);

        if (!bayIsValid)
        {
            return BadRequest(new
            {
                message = "The selected service bay is unavailable at this dealership."
            });
        }

        var dealershipExists = await _db.Dealerships
            .AnyAsync(
                d => d.DealershipId == request.DealershipId,
                cancellationToken);

        if (!dealershipExists)
        {
            return NotFound(new { message = "Dealership not found." });
        }

        var technicianHasConflict = await _db.Appointments
            .AnyAsync(
                a => a.TechnicianId == request.TechnicianId &&
                     a.Status != "Cancelled" &&
                     a.StartTimeUtc < endUtc &&
                     a.EndTimeUtc > startUtc,
                cancellationToken);

        if (technicianHasConflict)
        {
            return Conflict(new
            {
                message = "The technician is already booked for this time."
            });
        }

        var bayHasConflict = await _db.Appointments
            .AnyAsync(
                a => a.ServiceBayId == request.ServiceBayId &&
                     a.Status != "Cancelled" &&
                     a.StartTimeUtc < endUtc &&
                     a.EndTimeUtc > startUtc,
                cancellationToken);

        if (bayHasConflict)
        {
            return Conflict(new
            {
                message = "The service bay is already booked for this time."
            });
        }

        var appointment = new Appointment
        {
            CustomerId = request.CustomerId,
            VehicleId = request.VehicleId,
            DealershipId = request.DealershipId,
            ServiceTypeId = request.ServiceTypeId,
            TechnicianId = request.TechnicianId,
            ServiceBayId = request.ServiceBayId,
            StartTimeUtc = DateTime.SpecifyKind(
                startUtc, DateTimeKind.Utc),
            EndTimeUtc = DateTime.SpecifyKind(
                endUtc, DateTimeKind.Utc),
            CreatedAtUtc = DateTime.UtcNow,
            Status = "Confirmed"
        };

        _db.Appointments.Add(appointment);

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var response = new AppointmentResponse(
            appointment.AppointmentId,
            appointment.CustomerId,
            appointment.VehicleId,
            appointment.DealershipId,
            appointment.ServiceTypeId,
            appointment.TechnicianId,
            appointment.ServiceBayId,
            appointment.StartTimeUtc,
            appointment.EndTimeUtc,
            appointment.Status);

        return CreatedAtAction(
            nameof(GetAppointment),
            new { id = appointment.AppointmentId },
            response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAppointment(
        int id,
        CancellationToken cancellationToken)
    {
        var appointment = await _db.Appointments
            .AsNoTracking()
            .Where(a => a.AppointmentId == id)
            .Select(a => new AppointmentResponse(
                a.AppointmentId,
                a.CustomerId,
                a.VehicleId,
                a.DealershipId,
                a.ServiceTypeId,
                a.TechnicianId,
                a.ServiceBayId,
                a.StartTimeUtc,
                a.EndTimeUtc,
                a.Status))
            .SingleOrDefaultAsync(cancellationToken);

        if (appointment is null)
        {
            return NotFound(new { message = "Appointment not found." });
        }

        return Ok(appointment);
    }

    [HttpGet]
    public async Task<IActionResult> GetAppointments(
        CancellationToken cancellationToken)
    {
        var appointments = await _db.Appointments
            .AsNoTracking()
            .OrderByDescending(a => a.StartTimeUtc)
            .Select(a => new
            {
                appointmentId = a.AppointmentId,
                customerId = a.CustomerId,
                customerName = a.Customer.Name,
                customerEmail = a.Customer.Email,
                vehicleId = a.VehicleId,
                vehicleMake = a.Vehicle.Make,
                vehicleModel = a.Vehicle.Model,
                vehicleYear = a.Vehicle.Year,
                vehicleVin = a.Vehicle.VIN,
                dealershipId = a.DealershipId,
                dealershipName = a.Dealership.Name,
                serviceTypeId = a.ServiceTypeId,
                serviceName = a.ServiceType.Name,
                technicianId = a.TechnicianId,
                technicianName = a.Technician.Name,
                serviceBayId = a.ServiceBayId,
                serviceBayName = a.ServiceBay.BayName,
                startTimeUtc = a.StartTimeUtc,
                endTimeUtc = a.EndTimeUtc,
                status = a.Status
            })
            .ToListAsync(cancellationToken);

        return Ok(appointments);
    }

    [HttpPatch("{id:int}/cancel")]
    public async Task<IActionResult> CancelAppointment(
        int id,
        CancellationToken cancellationToken)
    {
        var appointment = await _db.Appointments
            .SingleOrDefaultAsync(
                a => a.AppointmentId == id,
                cancellationToken);

        if (appointment is null)
        {
            return NotFound(new
            {
                message = "Appointment not found."
            });
        }

        if (appointment.Status == "Cancelled")
        {
            return BadRequest(new
            {
                message = "Appointment is already cancelled."
            });
        }

        appointment.Status = "Cancelled";

        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            appointmentId = appointment.AppointmentId,
            status = appointment.Status,
            message = "Appointment cancelled successfully."
        });
    }


}

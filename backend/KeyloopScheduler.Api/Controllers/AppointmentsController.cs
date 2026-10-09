
using KeyloopScheduler.Api.Contracts;
using KeyloopScheduler.Api.Data;
using KeyloopScheduler.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

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

    // POST: api/appointments
    // Creates a new appointment after validating the customer,
    // vehicle, service, technician, service bay and booking conflicts.
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
                message = "The appointment start time must be in the future."
            });
        }

        await using var transaction =
            await _db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        // Validate customer.
        var customerExists = await _db.Customers.AnyAsync(
            c => c.CustomerId == request.CustomerId,
            cancellationToken);

        if (!customerExists)
        {
            return BadRequest(new
            {
                message = "The specified customer was not found."
            });
        }

        // Validate that the vehicle belongs to the selected customer.
        var vehicleExists = await _db.Vehicles.AnyAsync(
            v => v.VehicleId == request.VehicleId
                 && v.CustomerId == request.CustomerId,
            cancellationToken);

        if (!vehicleExists)
        {
            return BadRequest(new
            {
                message = "The selected vehicle does not belong to the specified customer."
            });
        }

        // Validate active service and retrieve its duration and skill.
        var service = await _db.ServiceTypes
            .AsNoTracking()
            .Where(s =>
                s.ServiceTypeId == request.ServiceTypeId
                && s.IsActive)
            .Select(s => new
            {
                s.ServiceTypeId,
                s.DurationMinutes,
                s.RequiredSkill
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (service is null)
        {
            return BadRequest(new
            {
                message = "The specified service type was not found or is inactive."
            });
        }

        if (service.DurationMinutes <= 0)
        {
            return Problem(
                title: "Invalid service configuration",
                detail: "The service duration must be greater than zero.",
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
                message = "The calculated appointment end time is out of range."
            });
        }

        // Validate dealership.
        var dealershipExists = await _db.Dealerships.AnyAsync(
            d => d.DealershipId == request.DealershipId,
            cancellationToken);

        if (!dealershipExists)
        {
            return NotFound(new
            {
                message = "The specified dealership was not found."
            });
        }

        // Validate technician, dealership and required skill.
        var technicianIsValid = await _db.Technicians.AnyAsync(
            t =>
                t.TechnicianId == request.TechnicianId
                && t.DealershipId == request.DealershipId
                && t.IsActive
                && t.Skills.Any(
                    skill => skill.SkillName == service.RequiredSkill),
            cancellationToken);

        if (!technicianIsValid)
        {
            return BadRequest(new
            {
                message = "The selected technician is inactive, not assigned to this dealership, or does not have the required skill."
            });
        }

        // Validate service bay and dealership.
        var serviceBayIsValid = await _db.ServiceBays.AnyAsync(
            b =>
                b.ServiceBayId == request.ServiceBayId
                && b.DealershipId == request.DealershipId
                && b.IsActive,
            cancellationToken);

        if (!serviceBayIsValid)
        {
            return BadRequest(new
            {
                message = "The selected service bay is inactive or does not belong to this dealership."
            });
        }

        // Check technician availability.
        // Cancelled appointments do not block a new booking.
        var technicianConflict = await _db.Appointments.AnyAsync(
            a =>
                a.TechnicianId == request.TechnicianId
                && a.Status != "Cancelled"
                && a.StartTimeUtc < endUtc
                && a.EndTimeUtc > startUtc,
            cancellationToken);

        if (technicianConflict)
        {
            return Conflict(new
            {
                message = "The technician is already booked for this time."
            });
        }

        // Check service bay availability.
        var serviceBayConflict = await _db.Appointments.AnyAsync(
            a =>
                a.ServiceBayId == request.ServiceBayId
                && a.Status != "Cancelled"
                && a.StartTimeUtc < endUtc
                && a.EndTimeUtc > startUtc,
            cancellationToken);

        if (serviceBayConflict)
        {
            return Conflict(new
            {
                message = "The service bay is already booked for this time."
            });
        }

        // Create the appointment using UTC timestamps.
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
            Status = "Confirmed",
            CreatedAtUtc = DateTime.UtcNow
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
            ToUtcOffset(appointment.StartTimeUtc),
            ToUtcOffset(appointment.EndTimeUtc),
            appointment.Status);

        return CreatedAtAction(
            nameof(GetAppointment),
            new { id = appointment.AppointmentId },
            response);
    }

    // GET: api/appointments/{id}
    // Retrieves one appointment.
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAppointment(
        int id,
        CancellationToken cancellationToken)
    {
        var appointment = await _db.Appointments
            .AsNoTracking()
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

        var response = new AppointmentResponse(
            appointment.AppointmentId,
            appointment.CustomerId,
            appointment.VehicleId,
            appointment.DealershipId,
            appointment.ServiceTypeId,
            appointment.TechnicianId,
            appointment.ServiceBayId,
            ToUtcOffset(appointment.StartTimeUtc),
            ToUtcOffset(appointment.EndTimeUtc),
            appointment.Status);

        return Ok(response);
    }

    // GET: api/appointments
    // Retrieves all appointments with details for the UI.
    [HttpGet]
    public async Task<IActionResult> GetAppointments(
        CancellationToken cancellationToken)
    {
        var appointments = await _db.Appointments
            .AsNoTracking()
            .OrderByDescending(a => a.StartTimeUtc)
            .Select(a => new
            {
                a.AppointmentId,
                a.CustomerId,
                CustomerName = a.Customer.Name,
                CustomerEmail = a.Customer.Email,
                a.VehicleId,
                VehicleMake = a.Vehicle.Make,
                VehicleModel = a.Vehicle.Model,
                VehicleYear = a.Vehicle.Year,
                VehicleVin = a.Vehicle.VIN,
                a.DealershipId,
                DealershipName = a.Dealership.Name,
                a.ServiceTypeId,
                ServiceName = a.ServiceType.Name,
                a.TechnicianId,
                TechnicianName = a.Technician.Name,
                a.ServiceBayId,
                ServiceBayName = a.ServiceBay.BayName,
                a.StartTimeUtc,
                a.EndTimeUtc,
                a.Status
            })
            .ToListAsync(cancellationToken);

        var response = appointments.Select(a => new
        {
            a.AppointmentId,
            a.CustomerId,
            a.CustomerName,
            a.CustomerEmail,
            a.VehicleId,
            a.VehicleMake,
            a.VehicleModel,
            a.VehicleYear,
            a.VehicleVin,
            a.DealershipId,
            a.DealershipName,
            a.ServiceTypeId,
            a.ServiceName,
            a.TechnicianId,
            a.TechnicianName,
            a.ServiceBayId,
            a.ServiceBayName,
            StartTimeUtc = ToUtcOffset(a.StartTimeUtc),
            EndTimeUtc = ToUtcOffset(a.EndTimeUtc),
            a.Status
        });

        return Ok(response);
    }

    // PATCH: api/appointments/{id}/cancel
    // Cancels an appointment without deleting its record.
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

    // Ensures database DateTime values are serialized with an explicit UTC offset.
    private static DateTimeOffset ToUtcOffset(DateTime value)
    {
        return new DateTimeOffset(
            DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }
}

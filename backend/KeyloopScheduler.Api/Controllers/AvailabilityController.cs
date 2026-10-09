
using KeyloopScheduler.Api.Contracts;
using KeyloopScheduler.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KeyloopScheduler.Api.Controllers;

[ApiController]
[Route("api/dealerships/{dealershipId:int}/availability")]
public sealed class AvailabilityController : ControllerBase
{
    private readonly SchedulerDbContext _db;

    public AvailabilityController(SchedulerDbContext db)
    {
        _db = db;
    }

    // GET: /api/dealerships/1/availability
    //     ?serviceTypeId=1&startTimeUtc=2026-10-10T09:00:00Z
    [HttpGet]
    public async Task<IActionResult> GetAvailability(
        int dealershipId,
        [FromQuery] int serviceTypeId,
        [FromQuery] DateTimeOffset startTimeUtc,
        CancellationToken cancellationToken)
    {
        if (serviceTypeId <= 0)
        {
            return BadRequest(new
            {
                message = "A valid serviceTypeId is required."
            });
        }

        var startUtc = startTimeUtc.UtcDateTime;

        if (startUtc <= DateTime.UtcNow)
        {
            return BadRequest(new
            {
                message = "The appointment start time must be in the future."
            });
        }

        var dealershipExists = await _db.Dealerships
            .AnyAsync(
                d => d.DealershipId == dealershipId,
                cancellationToken);

        if (!dealershipExists)
        {
            return NotFound(new
            {
                message = $"Dealership {dealershipId} was not found."
            });
        }

        var service = await _db.ServiceTypes
            .AsNoTracking()
            .Where(s =>
                s.ServiceTypeId == serviceTypeId &&
                s.IsActive)
            .Select(s => new
            {
                s.ServiceTypeId,
                s.DurationMinutes,
                s.RequiredSkill
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (service is null)
        {
            return NotFound(new
            {
                message = $"Active service type {serviceTypeId} was not found."
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
                message = "The requested appointment time is out of range."
            });
        }

        // Find active technicians who have the required qualification
        // and no conflicting appointment for the requested interval.
        var technicians = await _db.Technicians
            .AsNoTracking()
            .Where(t =>
                t.DealershipId == dealershipId &&
                t.IsActive &&
                t.Skills.Any(skill =>
                    skill.SkillName == service.RequiredSkill) &&
                !t.Appointments.Any(a =>
                    a.Status != "Cancelled" &&
                    a.StartTimeUtc < endUtc &&
                    a.EndTimeUtc > startUtc))
            .Select(t => new
            {
                t.TechnicianId,
                t.Name
            })
            .ToListAsync(cancellationToken);

        // Find active service bays with no conflicting appointment.
        var bays = await _db.ServiceBays
            .AsNoTracking()
            .Where(b =>
                b.DealershipId == dealershipId &&
                b.IsActive &&
                !b.Appointments.Any(a =>
                    a.Status != "Cancelled" &&
                    a.StartTimeUtc < endUtc &&
                    a.EndTimeUtc > startUtc))
            .Select(b => new
            {
                b.ServiceBayId,
                b.BayName
            })
            .ToListAsync(cancellationToken);

        // Return every available technician/bay combination.
        var availability =
            (from technician in technicians
             from bay in bays
             select new AvailabilityResponse(
                 technician.TechnicianId,
                 technician.Name,
                 bay.ServiceBayId,
                 bay.BayName,
                 DateTime.SpecifyKind(startUtc, DateTimeKind.Utc),
                 DateTime.SpecifyKind(endUtc, DateTimeKind.Utc)))
            .ToList();

        return Ok(availability);
    }
}

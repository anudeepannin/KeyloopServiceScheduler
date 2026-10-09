
using KeyloopScheduler.Api.Contracts;
using KeyloopScheduler.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KeyloopScheduler.Api.Controllers;

[ApiController]
[Route("api/dealerships")]
public sealed class DealershipsController : ControllerBase
{
    private readonly SchedulerDbContext _db;

    public DealershipsController(SchedulerDbContext db)
    {
        _db = db;
    }

    // GET: api/dealerships
    [HttpGet]
    public async Task<IActionResult> GetDealerships(
        CancellationToken cancellationToken)
    {
        var dealerships = await _db.Dealerships
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new
            {
                d.DealershipId,
                d.Name,
                d.Address,
                d.TimeZoneId
            })
            .ToListAsync(cancellationToken);

        return Ok(dealerships);
    }

    // GET: api/dealerships/{id}/service-types
    [HttpGet("{id:int}/service-types")]
    public async Task<IActionResult> GetServiceTypes(
        int id,
        CancellationToken cancellationToken)
    {
        var dealershipExists = await _db.Dealerships
            .AnyAsync(
                d => d.DealershipId == id,
                cancellationToken);

        if (!dealershipExists)
        {
            return NotFound(new
            {
                message = $"Dealership {id} was not found."
            });
        }

        var serviceTypes = await _db.ServiceTypes
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new ServiceTypeResponse(
                s.ServiceTypeId,
                s.Name,
                s.DurationMinutes,
                s.RequiredSkill))
            .ToListAsync(cancellationToken);

        return Ok(serviceTypes);
    }
}

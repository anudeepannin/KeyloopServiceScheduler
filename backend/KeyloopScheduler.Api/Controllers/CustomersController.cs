
using KeyloopScheduler.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KeyloopScheduler.Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly SchedulerDbContext _dbContext;

    public CustomersController(SchedulerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomers(
        CancellationToken cancellationToken)
    {
        var customers = await _dbContext.Customers
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new
            {
                customerId = c.CustomerId,
                name = c.Name,
                email = c.Email,
                phone = c.Phone
            })
            .ToListAsync(cancellationToken);

        return Ok(customers);
    }

    [HttpGet("{id:int}/vehicles")]
    public async Task<IActionResult> GetCustomerVehicles(
        int id,
        CancellationToken cancellationToken)
    {
        var customerExists = await _dbContext.Customers
            .AnyAsync(c => c.CustomerId == id, cancellationToken);

        if (!customerExists)
        {
            return NotFound(new { message = "Customer not found." });
        }

        var vehicles = await _dbContext.Vehicles
            .AsNoTracking()
            .Where(v => v.CustomerId == id)
            .OrderBy(v => v.Make)
            .Select(v => new
            {
                vehicleId = v.VehicleId,
                customerId = v.CustomerId,
                make = v.Make,
                model = v.Model,
                year = v.Year,
                vin = v.VIN
            })
            .ToListAsync(cancellationToken);

        return Ok(vehicles);
    }
}

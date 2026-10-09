
namespace KeyloopScheduler.Api.Entities;

public sealed class Vehicle
{
    public int VehicleId { get; set; }
    public int CustomerId { get; set; }
    public string VIN { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }

    public Customer Customer { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

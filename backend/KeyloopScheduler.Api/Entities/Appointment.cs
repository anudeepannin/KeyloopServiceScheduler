
namespace KeyloopScheduler.Api.Entities;

public sealed class Appointment
{
    public int AppointmentId { get; set; }

    public int CustomerId { get; set; }
    public int VehicleId { get; set; }
    public int DealershipId { get; set; }
    public int ServiceTypeId { get; set; }
    public int TechnicianId { get; set; }
    public int ServiceBayId { get; set; }

    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public string Status { get; set; } = "Confirmed";
    public DateTime CreatedAtUtc { get; set; }

    public Customer Customer { get; set; } = null!;
    public Vehicle Vehicle { get; set; } = null!;
    public Dealership Dealership { get; set; } = null!;
    public ServiceType ServiceType { get; set; } = null!;
    public Technician Technician { get; set; } = null!;
    public ServiceBay ServiceBay { get; set; } = null!;
}

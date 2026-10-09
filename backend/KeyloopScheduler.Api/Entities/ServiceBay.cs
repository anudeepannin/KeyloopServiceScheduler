
namespace KeyloopScheduler.Api.Entities;

public sealed class ServiceBay
{
    public int ServiceBayId { get; set; }
    public int DealershipId { get; set; }
    public string BayName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public Dealership Dealership { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

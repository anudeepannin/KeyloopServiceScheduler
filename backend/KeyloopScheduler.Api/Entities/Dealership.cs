
namespace KeyloopScheduler.Api.Entities;

public sealed class Dealership
{
    public int DealershipId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string TimeZoneId { get; set; } = "UTC";

    public ICollection<Technician> Technicians { get; set; } = new List<Technician>();
    public ICollection<ServiceBay> ServiceBays { get; set; } = new List<ServiceBay>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

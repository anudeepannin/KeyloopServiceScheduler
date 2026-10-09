
namespace KeyloopScheduler.Api.Entities;

public sealed class ServiceType
{
    public int ServiceTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string RequiredSkill { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

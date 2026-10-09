
namespace KeyloopScheduler.Api.Entities;

public sealed class Technician
{
    public int TechnicianId { get; set; }
    public int DealershipId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public Dealership Dealership { get; set; } = null!;

    public ICollection<TechnicianSkill> Skills { get; set; } = new List<TechnicianSkill>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

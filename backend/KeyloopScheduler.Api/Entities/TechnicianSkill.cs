
namespace KeyloopScheduler.Api.Entities;

public sealed class TechnicianSkill
{
    public int TechnicianSkillId { get; set; }
    public int TechnicianId { get; set; }
    public string SkillName { get; set; } = string.Empty;

    public Technician Technician { get; set; } = null!;
}

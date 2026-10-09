
namespace KeyloopScheduler.Api.Contracts;

public sealed record ServiceTypeResponse(
    int ServiceTypeId,
    string Name,
    int DurationMinutes,
    string RequiredSkill);

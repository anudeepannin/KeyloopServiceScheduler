
namespace KeyloopScheduler.Api.Contracts;

public sealed record AvailabilityResponse(
    int TechnicianId,
    string TechnicianName,
    int ServiceBayId,
    string BayName,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc);

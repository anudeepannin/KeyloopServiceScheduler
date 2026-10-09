
namespace KeyloopScheduler.Api.Contracts;

public sealed record AppointmentResponse(
    int AppointmentId,
    int CustomerId,
    int VehicleId,
    int DealershipId,
    int ServiceTypeId,
    int TechnicianId,
    int ServiceBayId,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    string Status);

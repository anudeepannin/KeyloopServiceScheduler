
using System.ComponentModel.DataAnnotations;

namespace KeyloopScheduler.Api.Contracts;

public sealed class CreateAppointmentRequest
{
    [Range(1, int.MaxValue)]
    public int CustomerId { get; set; }

    [Range(1, int.MaxValue)]
    public int VehicleId { get; set; }

    [Range(1, int.MaxValue)]
    public int DealershipId { get; set; }

    [Range(1, int.MaxValue)]
    public int ServiceTypeId { get; set; }

    [Range(1, int.MaxValue)]
    public int TechnicianId { get; set; }

    [Range(1, int.MaxValue)]
    public int ServiceBayId { get; set; }

    public DateTimeOffset StartTimeUtc { get; set; }
}

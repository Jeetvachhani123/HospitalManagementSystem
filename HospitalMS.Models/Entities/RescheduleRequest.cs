using HospitalMS.Models.Base;

namespace HospitalMS.Models.Entities;

public class RescheduleRequest : BaseEntity
{
    public int AppointmentId { get; set; }

    public DateTime RequestedDate { get; set; }

    public TimeSpan RequestedStartTime { get; set; }

    public TimeSpan RequestedEndTime { get; set; }

    public string Reason { get; set; } = string.Empty;

    public RescheduleRequestStatus Status { get; set; } = RescheduleRequestStatus.Pending;

    public string RequestedBy { get; set; } = string.Empty;

    public string? Response { get; set; }

    public DateTime? RespondedAt { get; set; }

    public int? RespondedByUserId { get; set; }

    public Appointment? Appointment { get; set; }
}

public enum RescheduleRequestStatus { Pending = 0, Approved = 1, Rejected = 2, Cancelled = 3 }
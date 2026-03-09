using HospitalMS.Models.Base;
using HospitalMS.Models.Enums;

namespace HospitalMS.Models.Entities;

public class AppointmentStatusHistory : BaseEntity
{
    public int AppointmentId { get; set; }

    public AppointmentStatus? PreviousStatus { get; set; }

    public AppointmentStatus NewStatus { get; set; }

    public AppointmentApprovalStatus? PreviousApprovalStatus { get; set; }

    public AppointmentApprovalStatus? NewApprovalStatus { get; set; }

    public string ChangedBy { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public string? ChangeReason { get; set; }

    public Appointment Appointment { get; set; } = null!;
}
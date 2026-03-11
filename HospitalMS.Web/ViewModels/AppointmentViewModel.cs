using HospitalMS.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace HospitalMS.Web.ViewModels;

public class AppointmentViewModel
{
    public int Id { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public string DoctorName { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;

    public DateTime AppointmentDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Reason { get; set; }

    public string? Diagnosis { get; set; }

    public string? Prescription { get; set; }

    public string? Notes { get; set; }

    public AppointmentStatus StatusEnum { get; set; }

    public string ApprovalStatus { get; set; } = string.Empty;

    public AppointmentApprovalStatus ApprovalStatusEnum { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }
}

public class AppointmentRequestViewModel
{
    public int DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;

    public decimal ConsultationFee { get; set; }

    [Required(ErrorMessage = "Please select an appointment date")]
    [DataType(DataType.Date)]
    public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

    [Required(ErrorMessage = "Please select a start time")]
    public TimeSpan StartTime { get; set; } = new TimeSpan(9, 0, 0);

    [Required(ErrorMessage = "Please select an end time")]
    public TimeSpan EndTime { get; set; } = new TimeSpan(10, 0, 0);

    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string? Reason { get; set; }
}

public class AppointmentDetailViewModel
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public string DoctorName { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;

    public DateTime AppointmentDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Reason { get; set; }

    public string? Diagnosis { get; set; }

    public string? Prescription { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<AppointmentTimelineItem> Timeline { get; set; } = new();

    public bool HasInvoice { get; set; }

    public int? InvoiceId { get; set; }

    public bool IsInvoicePaid { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }
}

public class AppointmentTimelineItem
{
    public DateTime DateTime { get; set; }

    public string Activity { get; set; } = string.Empty;

    public string User { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}

public class RescheduleAppointmentViewModel
{
    public int AppointmentId { get; set; }

    public int DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public DateTime CurrentDate { get; set; }

    public TimeSpan CurrentStartTime { get; set; }

    [Required(ErrorMessage = "Please select a new date")]
    [DataType(DataType.Date)]
    public DateTime NewDate { get; set; }

    [Required(ErrorMessage = "Please select a new start time")]
    public TimeSpan NewStartTime { get; set; }

    [Required(ErrorMessage = "Please select a new end time")]
    public TimeSpan NewEndTime { get; set; }
}

public class AppointmentCreateViewModel
{
    public int DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

    [Required]
    public TimeSpan StartTime { get; set; } = new TimeSpan(9, 0, 0);

    [Required]
    public TimeSpan EndTime { get; set; } = new TimeSpan(10, 0, 0);

    public string? Reason { get; set; }
}
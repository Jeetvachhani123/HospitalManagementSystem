using HospitalMS.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace HospitalMS.Models.Entities;

public class Doctor : AuditableEntity
{
    public int UserId { get; set; }

    public required string Specialization { get; set; }

    public required string LicenseNumber { get; set; }

    public int YearsOfExperience { get; set; }

    public string? Qualifications { get; set; }

    public string? Bio { get; set; }

    public decimal ConsultationFee { get; set; }

    public bool IsAvailable { get; set; } = true;

    public int AppointmentSlotDurationMinutes { get; set; } = 30;

    public int? DepartmentId { get; set; }

    public Department? Department { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public User User { get; set; } = null!;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public ICollection<DoctorWorkingHours> WorkingHours { get; set; } = new List<DoctorWorkingHours>();
}
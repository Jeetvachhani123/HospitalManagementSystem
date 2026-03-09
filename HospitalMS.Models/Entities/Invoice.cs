using HospitalMS.Models.Base;
using HospitalMS.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HospitalMS.Models.Entities;

public class Invoice : AuditableEntity
{
    public int AppointmentId { get; set; }

    public int PatientId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public DateTime IssueDate { get; set; }

    public DateTime DueDate { get; set; }

    public bool IsPaid { get; set; }

    public DateTime? PaidAt { get; set; }

    public string? PaymentMethod { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public Appointment Appointment { get; set; } = null!;

    public Patient Patient { get; set; } = null!;
}
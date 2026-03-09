using HospitalMS.Models.Base;
using HospitalMS.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace HospitalMS.Models.Entities;

public class MedicalRecord : AuditableEntity
{
    public int PatientId { get; set; }

    public int? DoctorId { get; set; }

    public DateTime RecordDate { get; set; }

    public string RecordType { get; set; } = string.Empty;

    public string? Diagnosis { get; set; }

    public string? Prescription { get; set; }

    public string? Notes { get; set; }

    public string? AttachmentPath { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public Patient Patient { get; set; } = null!;

    public Doctor? Doctor { get; set; }
}
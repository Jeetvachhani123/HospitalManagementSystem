namespace HospitalMS.BL.DTOs.MedicalRecord;

public class MedicalRecordCreateDto
{
    public int PatientId { get; set; }

    public int? DoctorId { get; set; }

    public DateTime RecordDate { get; set; }

    public string RecordType { get; set; } = string.Empty;

    public string? Diagnosis { get; set; }

    public string? Prescription { get; set; }

    public string? Notes { get; set; }

    public string? AttachmentPath { get; set; }
}
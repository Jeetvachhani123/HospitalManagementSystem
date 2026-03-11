namespace HospitalMS.BL.DTOs.MedicalRecord;

public class MedicalRecordDto
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public int? DoctorId { get; set; }

    public string? DoctorName { get; set; }
    
    public DateTime RecordDate { get; set; }

    public string RecordType { get; set; } = string.Empty;

    public string? Diagnosis { get; set; }

    public string? Prescription { get; set; }

    public string? Notes { get; set; }

    public string? AttachmentPath { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }
}
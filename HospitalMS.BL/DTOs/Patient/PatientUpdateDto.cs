namespace HospitalMS.BL.DTOs.Patient;

public class PatientUpdateDto
{
    public string? PhoneNumber { get; set; }

    public string? BloodGroup { get; set; }

    public string? Gender { get; set; }

    public string? EmergencyContact { get; set; }

    public string? MedicalHistory { get; set; }

    public string? Allergies { get; set; }

    public byte[]? RowVersion { get; set; }
}
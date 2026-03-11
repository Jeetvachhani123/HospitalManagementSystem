namespace HospitalMS.BL.DTOs.Patient;

public class PatientResponseDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public DateTime DateOfBirth { get; set; }

    public int Age { get; set; }

    public string? BloodGroup { get; set; }

    public string? Gender { get; set; }

    public string? EmergencyContact { get; set; }

    public string? MedicalHistory { get; set; }

    public string? Allergies { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }
}
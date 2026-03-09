namespace HospitalMS.BL.DTOs.Auth;

public class ProfileUpdateDto
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? BloodGroup { get; set; }

    public string? Street { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? ZipCode { get; set; }

    public string? EmergencyContact { get; set; }

    public string? MedicalHistory { get; set; }

    public string? Allergies { get; set; }

    public string? Specialization { get; set; }

    public string? LicenseNumber { get; set; }

    public int? YearsOfExperience { get; set; }

    public string? Qualifications { get; set; }

    public string? Bio { get; set; }

    public decimal? ConsultationFee { get; set; }
}

public class ChangePasswordDto
{
    public required string CurrentPassword { get; set; }

    public required string NewPassword { get; set; }

    public required string ConfirmNewPassword { get; set; }
}

public class UserProfileDto
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Role { get; set; } = string.Empty;

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? BloodGroup { get; set; }

    public string? Street { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? ZipCode { get; set; }

    public string? EmergencyContact { get; set; }

    public string? MedicalHistory { get; set; }

    public string? Allergies { get; set; }

    public string? Specialization { get; set; }

    public string? LicenseNumber { get; set; }

    public int? YearsOfExperience { get; set; }

    public string? Qualifications { get; set; }

    public string? Bio { get; set; }

    public decimal? ConsultationFee { get; set; }
}
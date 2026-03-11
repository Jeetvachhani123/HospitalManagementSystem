using System.ComponentModel.DataAnnotations;

namespace HospitalMS.Web.ViewModels;

public class PatientViewModel
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public int Age { get; set; }

    public string? BloodGroup { get; set; }

    public string? Gender { get; set; }

    public string? PhoneNumber { get; set; }

    public string? MedicalHistory { get; set; }

    public string? Allergies { get; set; }

    public string? EmergencyContact { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }
}

public class PatientRegistrationViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    public string? BloodGroup { get; set; }

    public string? Gender { get; set; }

    public string? PhoneNumber { get; set; }
}

public class PatientEditViewModel
{
    public int Id { get; set; }

    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Date of Birth")]
    public DateTime DateOfBirth { get; set; }

    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Blood Group")]
    public string? BloodGroup { get; set; }

    public string? Gender { get; set; }

    [Display(Name = "Emergency Contact")]
    public string? EmergencyContact { get; set; }

    [Display(Name = "Medical History")]
    public string? MedicalHistory { get; set; }

    public string? Allergies { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace HospitalMS.Web.ViewModels;

public class ProfileViewModel
{
    public int Id { get; set; }

    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Role")]
    public string Role { get; set; } = string.Empty;

    [Display(Name = "Last Login")]
    public DateTime? LastLoginAt { get; set; }

    [Display(Name = "Member Since")]
    public DateTime CreatedAt { get; set; }
}

public class ProfileEditViewModel
{
    public int Id { get; set; }

    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number format")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [Display(Name = "Gender")]
    public string? Gender { get; set; }

    [Display(Name = "Blood Group")]
    public string? BloodGroup { get; set; }

    [Display(Name = "Street Address")]
    public string? Street { get; set; }

    [Display(Name = "City")]
    public string? City { get; set; }

    [Display(Name = "State")]
    public string? State { get; set; }

    [Display(Name = "Zip Code")]
    public string? ZipCode { get; set; }

    [Display(Name = "Emergency Contact")]
    public string? EmergencyContact { get; set; }

    [Display(Name = "Medical History")]
    public string? MedicalHistory { get; set; }

    [Display(Name = "Allergies")]
    public string? Allergies { get; set; }

    [Display(Name = "Specialization")]
    public string? Specialization { get; set; }

    [Display(Name = "License Number")]
    public string? LicenseNumber { get; set; }

    [Display(Name = "Years of Experience")]
    public int? YearsOfExperience { get; set; }

    [Display(Name = "Qualifications")]
    public string? Qualifications { get; set; }

    [Display(Name = "Bio")]
    public string? Bio { get; set; }

    [Display(Name = "Consultation Fee")]
    public decimal? ConsultationFee { get; set; }
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Current password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your new password")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password")]
    [Compare("NewPassword", ErrorMessage = "The new passwords do not match")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
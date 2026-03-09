using HospitalMS.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace HospitalMS.Web.ViewModels;

public class DoctorViewModel
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;

    public string LicenseNumber { get; set; } = string.Empty;

    public int YearsOfExperience { get; set; }

    public decimal ConsultationFee { get; set; }

    public bool IsAvailable { get; set; }

    public string? Bio { get; set; }

    public string? Qualifications { get; set; }

    public string? PhoneNumber { get; set; }

    [Display(Name = "Department")]
    public string? DepartmentName { get; set; }

    public int? DepartmentId { get; set; }
}

public class DoctorCreateViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required, Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required, Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    [Required]
    public string Specialization { get; set; } = string.Empty;

    [Required, Display(Name = "License Number")]
    public string LicenseNumber { get; set; } = string.Empty;

    [Range(0, 100)]
    public int YearsOfExperience { get; set; }

    public decimal ConsultationFee { get; set; }

    public string? Qualifications { get; set; }

    public string? Bio { get; set; }

    [Display(Name = "Department")]
    public int? DepartmentId { get; set; }
}

public class DoctorEditViewModel
{
    public int Id { get; set; }

    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Required]
    public string Specialization { get; set; } = string.Empty;

    [Display(Name = "License Number")]
    public string LicenseNumber { get; set; } = string.Empty;

    [Range(0, 100), Display(Name = "Years of Experience")]
    public int YearsOfExperience { get; set; }

    [Display(Name = "Consultation Fee")]
    public decimal ConsultationFee { get; set; }

    public string? Qualifications { get; set; }

    public string? Bio { get; set; }

    [Display(Name = "Department")]
    public int? DepartmentId { get; set; }

    [Display(Name = "Available")]
    public bool IsAvailable { get; set; }
}
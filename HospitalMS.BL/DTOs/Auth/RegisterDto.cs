using HospitalMS.Models.Enums;

namespace HospitalMS.BL.DTOs.Auth;

public class RegisterDto
{
    public required string Email { get; set; }

    public required string Password { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public string? PhoneNumber { get; set; }

    public UserRole Role { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? BloodGroup { get; set; }
}
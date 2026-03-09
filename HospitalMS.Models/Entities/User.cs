using HospitalMS.Models.Base;
using HospitalMS.Models.Enums;

namespace HospitalMS.Models.Entities;

public class User : AuditableEntity
{
    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public string? PhoneNumber { get; set; }

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }

    public Doctor? Doctor { get; set; }

    public Patient? Patient { get; set; }

    public string GetFullName() => $"{FirstName} {LastName}";
}
using HospitalMS.Models.Base;
using HospitalMS.Models.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace HospitalMS.Models.Entities;

public class Patient : AuditableEntity
{
    public int UserId { get; set; }

    public DateTime DateOfBirth { get; set; }

    public string? BloodGroup { get; set; }

    public string? Gender { get; set; }

    public Address? Address { get; set; }

    public string? EmergencyContact { get; set; }

    public string? MedicalHistory { get; set; }

    public string? Allergies { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public User User { get; set; } = null!;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public int GetAge()
    {
        var today = DateTime.Today;
        var age = today.Year - DateOfBirth.Year;
        if (DateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }
}
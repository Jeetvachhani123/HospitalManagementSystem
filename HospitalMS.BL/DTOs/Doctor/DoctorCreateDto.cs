namespace HospitalMS.BL.DTOs.Doctor;

public class DoctorCreateDto
{
    public required string Email { get; set; }

    public required string Password { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public string? PhoneNumber { get; set; }

    public required string Specialization { get; set; }

    public required string LicenseNumber { get; set; }

    public int YearsOfExperience { get; set; }

    public string? Qualifications { get; set; }

    public string? Bio { get; set; }

    public decimal ConsultationFee { get; set; }

    public int? DepartmentId { get; set; }
}
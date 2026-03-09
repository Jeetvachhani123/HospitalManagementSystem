namespace HospitalMS.BL.DTOs.Doctor;

public class DoctorUpdateDto
{
    public string? PhoneNumber { get; set; }

    public string? Specialization { get; set; }

    public int? YearsOfExperience { get; set; }

    public string? Qualifications { get; set; }

    public string? Bio { get; set; }

    public decimal? ConsultationFee { get; set; }

    public bool? IsAvailable { get; set; }

    public byte[]? RowVersion { get; set; }

    public int? DepartmentId { get; set; }
}
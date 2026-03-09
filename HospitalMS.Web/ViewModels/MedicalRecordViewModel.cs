using System.ComponentModel.DataAnnotations;

namespace HospitalMS.Web.ViewModels;

public class MedicalRecordDisplayViewModel
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public string? DoctorName { get; set; }

    [Display(Name = "Date")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime RecordDate { get; set; }

    [Display(Name = "Type")]
    public string RecordType { get; set; } = string.Empty;

    public string? Diagnosis { get; set; }

    public string? Prescription { get; set; }

    public string? Notes { get; set; }

    public string? AttachmentPath { get; set; }
}

public class CreateMedicalRecordViewModel
{
    [Required]
    public int PatientId { get; set; }

    public string? PatientName { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Record Date")]
    public DateTime RecordDate { get; set; } = DateTime.Today;

    [Required]
    [Display(Name = "Record Type")]
    public string RecordType { get; set; } = "General Checkup";

    public string? Diagnosis { get; set; }

    [DataType(DataType.MultilineText)]
    public string? Prescription { get; set; }

    [DataType(DataType.MultilineText)]
    public string? Notes { get; set; }

    [Display(Name = "Attachment (PDF/Image)")]
    public IFormFile? Attachment { get; set; }
}
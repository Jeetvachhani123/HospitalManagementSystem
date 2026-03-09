using System.ComponentModel.DataAnnotations;

namespace HospitalMS.Web.ViewModels;

public class InvoiceViewModel
{
    public int Id { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public string DoctorName { get; set; } = string.Empty;

    public DateTime IssueDate { get; set; }

    public DateTime DueDate { get; set; }

    public decimal Amount { get; set; }

    public bool IsPaid { get; set; }

    public string StatusClass => IsPaid ? "bg-success" : (DueDate < DateTime.UtcNow ? "bg-danger" : "bg-warning");

    public string StatusText => IsPaid ? "Paid" : (DueDate < DateTime.UtcNow ? "Overdue" : "Pending");
}

public class GenerateInvoiceViewModel
{
    public int AppointmentId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public string DoctorName { get; set; } = string.Empty;

    public DateTime AppointmentDate { get; set; }

    [Required]
    [Range(0, 100000, ErrorMessage = "Amount must be between 0 and 100,000")]
    public decimal Amount { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(7);
}

public class PayInvoiceViewModel
{
    public int InvoiceId { get; set; }

    public decimal Amount { get; set; }

    public string PatientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a payment method")]
    public string PaymentMethod { get; set; } = string.Empty;

    public string CardNumber { get; set; } = string.Empty;

    public string ExpiryDate { get; set; } = string.Empty;

    public string CVV { get; set; } = string.Empty;
}
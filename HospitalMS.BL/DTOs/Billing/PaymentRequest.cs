using System.ComponentModel.DataAnnotations;

namespace HospitalMS.BL.DTOs.Billing;

public class PaymentRequest
{
    [Required]
    public string SuccessUrl { get; set; } = string.Empty;

    [Required]
    public string CancelUrl { get; set; } = string.Empty;
}

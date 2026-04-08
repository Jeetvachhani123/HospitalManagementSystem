using HospitalMS.BL.Common;
using HospitalMS.BL.Interfaces.Services;
using HospitalMS.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;
    private readonly IAppointmentService _appointmentService;
    private readonly ILogger<BillingController> _logger;
    public BillingController(IBillingService billingService, IAppointmentService appointmentService, ILogger<BillingController> logger)
    {
        _billingService = billingService;
        _appointmentService = appointmentService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<Invoice>>>> GetAll()
    {
        var invoices = await _billingService.GetAllInvoicesAsync();
        return Ok(ApiResponse<IEnumerable<Invoice>>.SuccessResponse(invoices));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<Invoice>>> GetById(int id)
    {
        var invoice = await _billingService.GetInvoiceByIdAsync(id);
        if (invoice == null)
        {
            return NotFound(ApiResponse<Invoice>.ErrorResponse("Invoice not found"));
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Patient")
        {
            var patient = await _appointmentService.GetPatientByUserIdAsync(userId);
            if (patient == null || invoice.PatientId != patient.Id)
            {
                return Forbid();
            }
        }

        return Ok(ApiResponse<Invoice>.SuccessResponse(invoice));
    }

    [HttpGet("patient/{patientId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IEnumerable<Invoice>>>> GetByPatientId(int patientId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Patient")
        {
            var patient = await _appointmentService.GetPatientByUserIdAsync(userId);
            if (patient == null || patient.Id != patientId)
            {
                return Forbid();
            }
        }

        var invoices = await _billingService.GetPatientInvoicesAsync(patientId);
        return Ok(ApiResponse<IEnumerable<Invoice>>.SuccessResponse(invoices));
    }

    [HttpGet("my-invoices")]
    [Authorize(Roles = "Patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<Invoice>>>> GetMyInvoices()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var patient = await _appointmentService.GetPatientByUserIdAsync(userId);
        if (patient == null)
        {
            return NotFound(ApiResponse<IEnumerable<Invoice>>.ErrorResponse("Patient profile not found"));
        }

        var invoices = await _billingService.GetPatientInvoicesAsync(patient.Id);
        return Ok(ApiResponse<IEnumerable<Invoice>>.SuccessResponse(invoices));
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<Invoice>>>> GetPendingInvoices()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var patient = await _appointmentService.GetPatientByUserIdAsync(userId);
        if (patient == null)
        {
            return NotFound(ApiResponse<IEnumerable<Invoice>>.ErrorResponse("Patient profile not found"));
        }

        var invoices = await _billingService.GetPendingInvoicesAsync(patient.Id);
        return Ok(ApiResponse<IEnumerable<Invoice>>.SuccessResponse(invoices));
    }

    [HttpPost("{id}/pay")]
    [Authorize(Roles = "Patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> ProcessPayment(int id, [FromBody] PaymentRequest request)
    {
        var invoice = await _billingService.GetInvoiceByIdAsync(id);
        if (invoice == null)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse("Invoice not found"));
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var patient = await _appointmentService.GetPatientByUserIdAsync(userId);
        if (patient == null || invoice.PatientId != patient.Id)
        {
            return Forbid();
        }

        if (invoice.IsPaid)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Invoice already paid"));
        }

        var result = await _billingService.ProcessPaymentAsync(id, request.PaymentMethod);
        if (!result)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Payment processing failed"));
        }

        _logger.LogInformation("Payment processed for invoice {InvoiceId} by patient {PatientId}", id, patient.Id);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Payment processed successfully"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> CancelInvoice(int id)
    {
        var result = await _billingService.CancelInvoiceAsync(id);
        if (!result)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse("Invoice not found or already cancelled"));
        }

        _logger.LogInformation("Invoice {InvoiceId} cancelled", id);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Invoice cancelled successfully"));
    }
}

public class PaymentRequest
{
    public string PaymentMethod { get; set; } = string.Empty;
}
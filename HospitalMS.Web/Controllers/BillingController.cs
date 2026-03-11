using HospitalMS.BL.Interfaces.Services;
using HospitalMS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalMS.Web.Controllers;

[Authorize]
public class BillingController : Controller
{
    private readonly IBillingService _billingService;
    private readonly IPatientService _patientService;
    private readonly IAppointmentService _appointmentService;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<BillingController> _logger;
    public BillingController(IBillingService billingService, IPatientService patientService, IAppointmentService appointmentService, IPaymentService paymentService, ILogger<BillingController> logger)
    {
        _billingService = billingService;
        _patientService = patientService;
        _appointmentService = appointmentService;
        _paymentService = paymentService;
        _logger = logger;
    }

    // list invoices
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var invoices = new List<InvoiceViewModel>();
        if (User.IsInRole("Admin"))
        {
            var allInvoices = await _billingService.GetAllInvoicesAsync();
            invoices = allInvoices.Select(i => new InvoiceViewModel
            {
                Id = i.Id,
                PatientName = i.Patient?.User?.GetFullName() ?? "Unknown",
                DoctorName = i.Appointment?.Doctor?.User?.GetFullName() ?? "Unknown",
                IssueDate = i.IssueDate,
                DueDate = i.DueDate,
                Amount = i.Amount,
                IsPaid = i.IsPaid
            }).ToList();
        }
        else if (User.IsInRole("Patient"))
        {
            var patient = await _patientService.GetByUserIdAsync(userId);
            if (patient != null)
            {
                var patientInvoices = await _billingService.GetPatientInvoicesAsync(patient.Id);
                invoices = patientInvoices.Select(i => new InvoiceViewModel
                {
                    Id = i.Id,
                    PatientName = i.Patient?.User?.GetFullName() ?? "Unknown",
                    DoctorName = i.Appointment?.Doctor?.User?.GetFullName() ?? "Unknown",
                    IssueDate = i.IssueDate,
                    DueDate = i.DueDate,
                    Amount = i.Amount,
                    IsPaid = i.IsPaid
                }).ToList();
            }
        }
        return View(invoices);
    }

    // invoice details
    [HttpGet]
    [Authorize(Roles = "Admin,Patient")]
    public async Task<IActionResult> Details(int id)
    {
        var invoice = await _billingService.GetInvoiceByIdAsync(id);
        if (invoice == null) return NotFound();
        if (User.IsInRole("Patient"))
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var patient = await _patientService.GetByUserIdAsync(userId);
            if (patient == null || invoice.PatientId != patient.Id)
            {
                return Forbid();
            }
        }
        var model = new InvoiceViewModel
        {
            Id = invoice.Id,
            PatientName = invoice.Patient.User?.GetFullName() ?? "Unknown",
            DoctorName = invoice.Appointment.Doctor.User?.GetFullName() ?? "Unknown",
            IssueDate = invoice.IssueDate,
            DueDate = invoice.DueDate,
            Amount = invoice.Amount,
            IsPaid = invoice.IsPaid,
            CreatedBy = invoice.CreatedBy,
            UpdatedBy = invoice.UpdatedBy
        };
        return View(model);
    }

    // show create invoice form
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(int appointmentId)
    {
        var existingInvoice = await _billingService.GetInvoiceByAppointmentIdAsync(appointmentId);
        if (existingInvoice != null)
        {
            TempData["InfoMessage"] = "Invoice already exists for this appointment.";
            return RedirectToAction(nameof(Details), new { id = existingInvoice.Id });
        }
        var appointment = await _appointmentService.GetByIdAsync(appointmentId);
        if (appointment == null) return NotFound();
        var model = new GenerateInvoiceViewModel
        {
            AppointmentId = appointmentId,
            PatientName = appointment.PatientName,
            DoctorName = appointment.DoctorName,
            AppointmentDate = appointment.AppointmentDate,
            Amount = 100
        };
        return View(model);
    }

    // generate invoice
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GenerateInvoiceViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            await _billingService.GenerateInvoiceAsync(model.AppointmentId, model.Amount, model.DueDate);
            TempData["SuccessMessage"] = "Invoice generated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice");
            ModelState.AddModelError("", "Failed to generate invoice: " + ex.Message);
            return View(model);
        }
    }

    // show payment form
    [HttpGet]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> Pay(int id)
    {
        var invoice = await _billingService.GetInvoiceByIdAsync(id);
        if (invoice == null)
            return NotFound();
        if (invoice.IsPaid)
            return RedirectToAction(nameof(Details), new { id });
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var patient = await _patientService.GetByUserIdAsync(userId);
        if (patient == null || invoice.PatientId != patient.Id)
        {
            return Forbid();
        }
        var model = new PayInvoiceViewModel
        {
            InvoiceId = invoice.Id,
            Amount = invoice.Amount,
            PatientName = invoice.Patient?.User?.GetFullName() ?? "Unknown"
        };
        return View(model);
    }

    // process payment
    [HttpPost]
    [Authorize(Roles = "Patient")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay(PayInvoiceViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        if (model.PaymentMethod == "Credit Card")
        {
            if (string.IsNullOrWhiteSpace(model.CardNumber) || string.IsNullOrWhiteSpace(model.ExpiryDate) || string.IsNullOrWhiteSpace(model.CVV))
            {
                ModelState.AddModelError("", "Please enter card details.");
                return View(model);
            }
        }
        try
        {
            if (model.PaymentMethod == "Credit Card")
            {
                var successUrl = Url.Action("PaymentSuccess", "Billing", new { id = model.InvoiceId }, Request.Scheme) ?? string.Empty;
                var cancelUrl = Url.Action("Details", "Billing", new { id = model.InvoiceId }, Request.Scheme) ?? string.Empty;
                var checkoutUrl = await _paymentService.CreateCheckoutSessionAsync(model.InvoiceId, model.Amount, "usd", successUrl, cancelUrl);
                return Redirect(checkoutUrl);
            }
            else
            {
                var success = await _billingService.ProcessPaymentAsync(model.InvoiceId, model.PaymentMethod);
                if (success)
                {
                    TempData["SuccessMessage"] = "Payment processed successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Payment processing failed.");
                    return View(model);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment");
            ModelState.AddModelError("", "An error occurred during payment.");
            return View(model);
        }
    }

    // stripe payment success redirect
    [HttpGet]
    [Authorize(Roles = "Patient")]
    public IActionResult PaymentSuccess(int id)
    {
        TempData["SuccessMessage"] = "Payment processed successfully via Stripe!";
        return RedirectToAction(nameof(Details), new { id });
    }

    // stripe webhook handler
    [HttpPost]
    [AllowAnonymous]
    [Route("webhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].ToString();
        try
        {
            var result = await _paymentService.HandleWebhookAsync(json, signature ?? string.Empty);
            if (result) return Ok();
            else return BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe webhook failed");
            return BadRequest();
        }
    }
}
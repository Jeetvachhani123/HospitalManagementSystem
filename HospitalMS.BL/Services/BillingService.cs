using HospitalMS.DATA.UnitOfWork;
using HospitalMS.Models.Entities;
using HospitalMS.Models.Enums;
using Microsoft.Extensions.Logging;

namespace HospitalMS.BL.Services;

public interface IBillingService
{
    Task<Invoice?> GetInvoiceByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Invoice?> GetInvoiceByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetAllInvoicesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetPatientInvoicesAsync(int patientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetPendingInvoicesAsync(int patientId, CancellationToken cancellationToken = default);
    Task<Invoice> GenerateInvoiceAsync(int appointmentId, decimal amount, DateTime dueDate, CancellationToken cancellationToken = default);
    Task<bool> ProcessPaymentAsync(int invoiceId, string paymentMethod, CancellationToken cancellationToken = default);
    Task<bool> CancelInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default);
}

public class BillingService : IBillingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BillingService> _logger;
    public BillingService(IUnitOfWork unitOfWork, ILogger<BillingService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Invoice?> GetInvoiceByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Invoices.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Invoice?> GetInvoiceByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Invoices.GetByAppointmentIdAsync(appointmentId, cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync(CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Invoices.GetAllAsync(cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetPatientInvoicesAsync(int patientId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Invoices.GetByPatientIdAsync(patientId, cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetPendingInvoicesAsync(int patientId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Invoices.GetPendingByPatientIdAsync(patientId, cancellationToken);
    }

    public async Task<Invoice> GenerateInvoiceAsync(int appointmentId, decimal amount, DateTime dueDate, CancellationToken cancellationToken = default)
    {
        var existingInvoice = await _unitOfWork.Invoices.GetByAppointmentIdAsync(appointmentId, cancellationToken);
        if (existingInvoice != null)
            throw new InvalidOperationException($"Invoice already exists for appointment {appointmentId}");

        var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment == null)
            throw new InvalidOperationException($"Appointment {appointmentId} not found");

        var invoice = new Invoice { AppointmentId = appointmentId, PatientId = appointment.PatientId, Amount = amount, IssueDate = DateTime.UtcNow, DueDate = dueDate, IsPaid = false };
        await _unitOfWork.Invoices.AddAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Invoice {InvoiceId} generated for Appointment {AppointmentId}", invoice.Id, appointmentId);
        return invoice;
    }

    public async Task<bool> ProcessPaymentAsync(int invoiceId, string paymentMethod, CancellationToken cancellationToken = default)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId, cancellationToken);
        if (invoice == null)
            return false;

        if (invoice.IsPaid)
        {
            _logger.LogWarning("Attempted to pay already paid invoice {InvoiceId}", invoiceId);
            return false;
        }

        invoice.IsPaid = true;
        invoice.PaidAt = DateTime.UtcNow;
        invoice.PaymentMethod = paymentMethod;
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Invoice {InvoiceId} paid via {PaymentMethod}", invoiceId, paymentMethod);
            return true;
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateConcurrencyException")
        {
            _logger.LogWarning("Concurrency conflict while processing payment for invoice {InvoiceId}", invoiceId);
            throw new HospitalMS.BL.Exceptions.ConcurrencyException("Invoice", invoiceId);
        }
    }

    public async Task<bool> CancelInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId, cancellationToken);
        if (invoice == null)
            return false;

        if (invoice.IsPaid)
        {
            _logger.LogWarning("Cannot cancel paid invoice {InvoiceId}", invoiceId);
            return false;
        }
        _unitOfWork.Invoices.Delete(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Invoice {InvoiceId} cancelled", invoiceId);
        return true;
    }
}
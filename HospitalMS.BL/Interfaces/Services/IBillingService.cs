using HospitalMS.Models.Entities;

namespace HospitalMS.BL.Interfaces.Services;

public interface IBillingService
{
    Task<Invoice?> GetInvoiceByIdAsync(int id);
    
    Task<Invoice?> GetInvoiceByAppointmentIdAsync(int appointmentId);

    Task<IEnumerable<Invoice>> GetAllInvoicesAsync();

    Task<IEnumerable<Invoice>> GetPatientInvoicesAsync(int patientId);

    Task<IEnumerable<Invoice>> GetPendingInvoicesAsync(int patientId);

    Task<Invoice> GenerateInvoiceAsync(int appointmentId, decimal amount, DateTime dueDate);

    Task<bool> ProcessPaymentAsync(int invoiceId, string paymentMethod);

    Task<bool> CancelInvoiceAsync(int invoiceId);
}
using HospitalMS.Models.Entities;

namespace HospitalMS.BL.Interfaces.Repositories;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(int id);

    Task<IEnumerable<Invoice>> GetAllAsync();

    Task<IEnumerable<Invoice>> GetByPatientIdAsync(int patientId);

    Task<Invoice?> GetByAppointmentIdAsync(int appointmentId);

    Task<IEnumerable<Invoice>> GetPendingByPatientIdAsync(int patientId);

    Task AddAsync(Invoice invoice);

    void Update(Invoice invoice);

    void Delete(Invoice invoice);
}
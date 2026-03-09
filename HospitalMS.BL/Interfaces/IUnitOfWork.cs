using HospitalMS.BL.Interfaces.Repositories;

namespace HospitalMS.BL.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }

    IDoctorRepository Doctors { get; }

    IPatientRepository Patients { get; }

    IAppointmentRepository Appointments { get; }

    IDoctorWorkingHoursRepository DoctorWorkingHours { get; }

    IInvoiceRepository Invoices { get; }

    IMedicalRecordRepository MedicalRecords { get; }

    IDepartmentRepository Departments { get; }

    IAppointmentStatusHistoryRepository AppointmentStatusHistories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    int SaveChanges();

    Task<System.Data.IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
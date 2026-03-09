using HospitalMS.BL.Interfaces;
using HospitalMS.BL.Interfaces.Repositories;
using HospitalMS.DATA.Context;
using HospitalMS.DATA.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalMS.DATA.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly HospitalDbContext _context;
    private readonly IServiceProvider _serviceProvider;
    private IUserRepository? _userRepository;
    private IDoctorRepository? _doctorRepository;
    private IPatientRepository? _patientRepository;
    private IAppointmentRepository? _appointmentRepository;
    private IDoctorWorkingHoursRepository? _doctorWorkingHoursRepository;
    private IInvoiceRepository? _invoiceRepository;
    private IMedicalRecordRepository? _medicalRecordRepository;
    private IDepartmentRepository? _departmentRepository;
    private IAppointmentStatusHistoryRepository? _appointmentStatusHistoryRepository;

    public UnitOfWork(HospitalDbContext context, IServiceProvider serviceProvider)
    {
        _context = context;
        _serviceProvider = serviceProvider;
    }

    public IUserRepository Users => _userRepository ??= _serviceProvider.GetRequiredService<IUserRepository>();

    public IDoctorRepository Doctors => _doctorRepository ??= _serviceProvider.GetRequiredService<IDoctorRepository>();

    public IPatientRepository Patients => _patientRepository ??= _serviceProvider.GetRequiredService<IPatientRepository>();

    public IAppointmentRepository Appointments => _appointmentRepository ??= _serviceProvider.GetRequiredService<IAppointmentRepository>();

    public IDoctorWorkingHoursRepository DoctorWorkingHours => _doctorWorkingHoursRepository ??= _serviceProvider.GetRequiredService<IDoctorWorkingHoursRepository>();

    public IInvoiceRepository Invoices => _invoiceRepository ??= _serviceProvider.GetRequiredService<IInvoiceRepository>();

    public IMedicalRecordRepository MedicalRecords => _medicalRecordRepository ??= _serviceProvider.GetRequiredService<IMedicalRecordRepository>();

    public IDepartmentRepository Departments => _departmentRepository ??= _serviceProvider.GetRequiredService<IDepartmentRepository>();

    public IAppointmentStatusHistoryRepository AppointmentStatusHistories => _appointmentStatusHistoryRepository ??= _serviceProvider.GetRequiredService<IAppointmentStatusHistoryRepository>();

    public async Task<System.Data.IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return transaction.GetDbTransaction();
    }

    // save changes async
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    // save changes sync
    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    // dispose context
    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
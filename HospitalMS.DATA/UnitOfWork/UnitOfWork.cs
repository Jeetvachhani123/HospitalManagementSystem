using HospitalMS.DATA.Context;
using HospitalMS.DATA.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalMS.DATA.UnitOfWork;

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
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);
    Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
}

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

    public Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            var result = await action();
            await transaction.CommitAsync(cancellationToken);
            return result;
        });
    }

    public Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            await action();
            await transaction.CommitAsync(cancellationToken);
        });
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
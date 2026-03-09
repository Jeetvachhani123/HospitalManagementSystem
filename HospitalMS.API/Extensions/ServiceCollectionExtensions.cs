using HospitalMS.BL.Interfaces;
using HospitalMS.BL.Interfaces.Repositories;
using HospitalMS.BL.Interfaces.Services;
using HospitalMS.BL.Services;
using HospitalMS.DATA.Context;
using HospitalMS.DATA.Repositories;
using HospitalMS.DATA.UnitOfWork;
using HospitalMS.API.Services;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<HospitalDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IDoctorWorkingHoursRepository, DoctorWorkingHoursRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IAppointmentStatusHistoryRepository, AppointmentStatusHistoryRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IAppointmentWorkflowService, AppointmentWorkflowService>();
        services.AddScoped<IBillingService, BillingService>();
        services.AddScoped<IAppointmentNotificationService, AppointmentNotificationService>();
        services.AddScoped<IMedicalRecordService, MedicalRecordService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IWorkingHoursService, WorkingHoursService>();
        return services;
    }
}
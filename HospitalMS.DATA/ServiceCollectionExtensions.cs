using HospitalMS.BL.Interfaces;
using HospitalMS.BL.Interfaces.Repositories;
using HospitalMS.BL.Interfaces.Services;
using HospitalMS.BL.Services;
using HospitalMS.DATA.Context;
using HospitalMS.DATA.Repositories;
using HospitalMS.DATA.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalMS.DATA;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHospitalServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<HospitalDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 10,
                    maxRetryDelay: TimeSpan.FromSeconds(60),
                    errorNumbersToAdd: new[] { -2, 233, 4060, 40197, 40501, 40613, 49918, 49919, 49920 });
                sqlOptions.CommandTimeout(180);
            })
            .ConfigureWarnings(w => w.Log(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.CommandExecuted))
        );
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IDoctorWorkingHoursRepository, DoctorWorkingHoursRepository>();
        services.AddScoped<IAppointmentStatusHistoryRepository, AppointmentStatusHistoryRepository>();
        return services;
    }
}
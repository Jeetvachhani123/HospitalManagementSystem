using HospitalMS.API.Extensions;
using HospitalMS.DATA;
using HospitalMS.API.Filters;
using HospitalMS.API.Hubs;
using HospitalMS.API.Middlewares;
using HospitalMS.API.Services;
using HospitalMS.BL.Common;
using HospitalMS.BL.Mappings;
using HospitalMS.BL.Interfaces.Services;
using HospitalMS.BL.Services;
using FluentValidation;
using Serilog;
using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Host.UseSerilog();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<AppointmentSettings>(builder.Configuration.GetSection("AppointmentSettings"));
builder.Services.AddHospitalServices(builder.Configuration);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IRealTimeNotificationService, RealTimeNotificationService>();
builder.Services.AddScoped<IAppointmentNotificationService, AppointmentNotificationService>();

// Decoupled from DATA layer
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<UserRegistrationCoordinator>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IAppointmentWorkflowService, AppointmentWorkflowService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IWorkingHoursService, WorkingHoursService>();
builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();

// Add user context accessor
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<HospitalMS.BL.Interfaces.ICurrentUserService, CurrentUserService>();
builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
builder.Services.AddValidatorsFromAssemblyContaining(typeof(HospitalMS.BL.Validators.DoctorCreateValidator));
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
    options.Filters.Add<ValidateModelFilter>();
});
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!);
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .WithOrigins(
                "http://localhost:3000",
                "https://localhost:7058",
                "http://localhost:5000",
                "https://localhost:5001")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();
app.UseSerilogRequestLogging();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionMiddleware>();
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");
app.MapHealthChecks("/health");
app.Run();
using HospitalMS.DATA;
using HospitalMS.BL.Common;
using HospitalMS.BL.Mappings;
using HospitalMS.BL.Interfaces;
using HospitalMS.BL.Interfaces.Repositories;
using HospitalMS.BL.Interfaces.Services;
using HospitalMS.BL.Services;
using HospitalMS.DATA.Context;
using HospitalMS.DATA.UnitOfWork;
using HospitalMS.DATA.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using AutoMapper;
using HospitalMS.Web.Hubs;
using HospitalMS.Web.Middleware;
using HospitalMS.Web.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHospitalServices(builder.Configuration);
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IPaymentService, StripePaymentService>();
builder.Services.AddScoped<IRealTimeNotificationService, SignalRNotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();

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
builder.Services.AddHttpClient<ApiClient>()
    .ConfigureHttpClient((provider, client) =>
    {
        var config = provider.GetRequiredService<IConfiguration>();
        var apiBaseUrl = config["ApiSettings:BaseUrl"] ?? "https://localhost:7058";
        client.BaseAddress = new Uri(apiBaseUrl);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });
builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
builder.Services.AddValidatorsFromAssemblyContaining(typeof(HospitalMS.BL.Validators.DoctorCreateValidator));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
    });
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<AppointmentSettings>(builder.Configuration.GetSection("AppointmentSettings"));
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseStaticFiles();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<RateLimitingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.MapHub<NotificationHub>("/notificationHub");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
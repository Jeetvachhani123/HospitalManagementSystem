using HospitalMS.BL.DTOs.Auth;
using HospitalMS.BL.Interfaces;
using HospitalMS.BL.Interfaces.Services;
using HospitalMS.Models.Entities;
using HospitalMS.Models.Enums;
using Microsoft.Extensions.Logging;

namespace HospitalMS.BL.Services;

public class UserRegistrationCoordinator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserRegistrationCoordinator> _logger;
    public UserRegistrationCoordinator(IUnitOfWork unitOfWork, ILogger<UserRegistrationCoordinator> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // register new user
    public async Task<User?> RegisterUserAsync(RegisterDto registerDto, Func<string, string> passwordHasher)
    {
        if (await _unitOfWork.Users.EmailExistsAsync(registerDto.Email))
        {
            _logger.LogWarning("Registration failed: Email {Email} already exists", registerDto.Email);
            return null;
        }
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var user = new User { Email = registerDto.Email, PasswordHash = passwordHasher(registerDto.Password), FirstName = registerDto.FirstName, LastName = registerDto.LastName, PhoneNumber = registerDto.PhoneNumber, Role = UserRole.Patient, IsActive = true };
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            await CreatePatientProfileAsync(user, registerDto);
            await _unitOfWork.SaveChangesAsync();
            transaction.Commit();
            _logger.LogInformation("Successfully registered user {UserId} ({Email}) with role {Role}", user.Id, user.Email, user.Role);
            return user;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Failed to create profile for user. Rolled back transaction.");
            throw;
        }
    }

    // create patient profile
    private async Task CreatePatientProfileAsync(User user, RegisterDto dto)
    {
        var patient = new Patient
        {
            UserId = user.Id,
            DateOfBirth = dto.DateOfBirth ?? DateTime.UtcNow.AddYears(-25),
            Gender = dto.Gender ?? "Not Specified",
            BloodGroup = dto.BloodGroup,
            EmergencyContact = dto.PhoneNumber,
            Address = new HospitalMS.Models.ValueObjects.Address()
        };
        await _unitOfWork.Patients.AddAsync(patient);
    }

    // create doctor profile
    private async Task CreateDoctorProfileAsync(User user, RegisterDto dto)
    {
        var doctor = new Doctor
        {
            UserId = user.Id,
            Specialization = "General",
            LicenseNumber = "PENDING-" + Guid.NewGuid().ToString().Substring(0, 8),
            ConsultationFee = 0,
            IsAvailable = false
        };
        await _unitOfWork.Doctors.AddAsync(doctor);
    }
}
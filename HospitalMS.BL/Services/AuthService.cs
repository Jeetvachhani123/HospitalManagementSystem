using AutoMapper;
using HospitalMS.BL.Common;
using HospitalMS.BL.DTOs.Auth;
using HospitalMS.BL.Interfaces;
using HospitalMS.BL.Interfaces.Services;
using HospitalMS.Models.Entities;
using HospitalMS.Models.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HospitalMS.BL.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;
    private readonly UserRegistrationCoordinator _registrationCoordinator;
    public AuthService(IUnitOfWork unitOfWork, IMapper mapper, IOptions<JwtSettings> jwtSettings, ILogger<AuthService> logger, UserRegistrationCoordinator registrationCoordinator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
        _registrationCoordinator = registrationCoordinator;
    }

    // login user
    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        _logger.LogInformation("Login attempt for email: {Email}", loginDto.Email);
        var user = await _unitOfWork.Users.GetByEmailAsync(loginDto.Email);
        if (user == null)
        {
            _logger.LogWarning("Login failed: User not found with email: {Email}", loginDto.Email);
            return null;
        }
        _logger.LogInformation("User found: {UserId}, IsActive: {IsActive}, Email: {Email}", user.Id, user.IsActive, user.Email);
        if (!VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: Invalid password for user {UserId} ({Email})", user.Id, loginDto.Email);
            return null;
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed: User {UserId} ({Email}) is inactive", user.Id, loginDto.Email);
            return null;
        }

        _logger.LogInformation("Login successful for user {UserId} ({Email})", user.Id, loginDto.Email);
        user.LastLoginAt = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        var token = GenerateJwtToken(user);
        return new AuthResponseDto { Token = token, UserId = user.Id, Email = user.Email, FirstName = user.FirstName, LastName = user.LastName, Role = user.Role.ToString(), ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes) };
    }

    // register user
    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
    {
        var user = await _registrationCoordinator.RegisterUserAsync(registerDto, HashPassword);
        if (user == null)
        {
            return null;
        }

        var token = GenerateJwtToken(user);
        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes)
        };
    }

    // validate jwt token
    public Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    // generate jwt token
    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Email, user.Email), new Claim(ClaimTypes.Name, user.GetFullName()), new Claim(ClaimTypes.Role, user.Role.ToString()) };
        var tokenDescriptor = new SecurityTokenDescriptor { Subject = new ClaimsIdentity(claims), Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes), Issuer = _jwtSettings.Issuer, Audience = _jwtSettings.Audience, SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature) };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    // get user profile
    public async Task<UserProfileDto?> GetProfileAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) 
            return null;

        var profile = new UserProfileDto { Id = user.Id, Email = user.Email, FirstName = user.FirstName, LastName = user.LastName, FullName = user.GetFullName(), PhoneNumber = user.PhoneNumber, Role = user.Role.ToString(), LastLoginAt = user.LastLoginAt, CreatedAt = user.CreatedAt };
        if (user.Role == UserRole.Patient)
        {
            var patient = await _unitOfWork.Patients.GetByUserIdAsync(userId);
            if (patient != null)
            {
                profile.DateOfBirth = patient.DateOfBirth;
                profile.Gender = patient.Gender;
                profile.BloodGroup = patient.BloodGroup;
                profile.Street = patient.Address?.Street;
                profile.City = patient.Address?.City;
                profile.State = patient.Address?.State;
                profile.ZipCode = patient.Address?.ZipCode;
                profile.EmergencyContact = patient.EmergencyContact;
                profile.MedicalHistory = patient.MedicalHistory;
                profile.Allergies = patient.Allergies;
            }
        }
        else if (user.Role == UserRole.Doctor)
        {
            var doctor = await _unitOfWork.Doctors.GetByUserIdAsync(userId);
            if (doctor != null)
            {
                profile.Specialization = doctor.Specialization;
                profile.LicenseNumber = doctor.LicenseNumber;
                profile.YearsOfExperience = doctor.YearsOfExperience;
                profile.Qualifications = doctor.Qualifications;
                profile.Bio = doctor.Bio;
                profile.ConsultationFee = doctor.ConsultationFee;
            }
        }
        return profile;
    }

    // update user profile
    public async Task<UserProfileDto?> UpdateProfileAsync(int userId, ProfileUpdateDto profileDto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) 
            return null;

        if (!string.IsNullOrWhiteSpace(profileDto.FirstName)) user.FirstName = profileDto.FirstName;
        
        if (!string.IsNullOrWhiteSpace(profileDto.LastName)) user.LastName = profileDto.LastName;
        
        if (profileDto.PhoneNumber != null) user.PhoneNumber = profileDto.PhoneNumber;
        _unitOfWork.Users.Update(user);
        if (user.Role == UserRole.Patient)
        {
            var patient = await _unitOfWork.Patients.GetByUserIdAsync(userId);
            if (patient != null)
            {
                if (profileDto.DateOfBirth.HasValue) patient.DateOfBirth = profileDto.DateOfBirth.Value;
                if (!string.IsNullOrWhiteSpace(profileDto.Gender)) patient.Gender = profileDto.Gender;
                if (!string.IsNullOrWhiteSpace(profileDto.BloodGroup)) patient.BloodGroup = profileDto.BloodGroup;
                if (patient.Address == null) patient.Address = new HospitalMS.Models.ValueObjects.Address();
                if (profileDto.Street != null) patient.Address.Street = profileDto.Street;
                if (profileDto.City != null) patient.Address.City = profileDto.City;
                if (profileDto.State != null) patient.Address.State = profileDto.State;
                if (profileDto.ZipCode != null) patient.Address.ZipCode = profileDto.ZipCode;
                if (profileDto.EmergencyContact != null) patient.EmergencyContact = profileDto.EmergencyContact;
                if (profileDto.MedicalHistory != null) patient.MedicalHistory = profileDto.MedicalHistory;
                if (profileDto.Allergies != null) patient.Allergies = profileDto.Allergies;
                _unitOfWork.Patients.Update(patient);
            }
        }
        else if (user.Role == UserRole.Doctor)
        {
            var doctor = await _unitOfWork.Doctors.GetByUserIdAsync(userId);
            if (doctor != null)
            {
                if (!string.IsNullOrWhiteSpace(profileDto.Specialization)) doctor.Specialization = profileDto.Specialization;
                if (!string.IsNullOrWhiteSpace(profileDto.LicenseNumber)) doctor.LicenseNumber = profileDto.LicenseNumber;
                if (profileDto.YearsOfExperience.HasValue) doctor.YearsOfExperience = profileDto.YearsOfExperience.Value;
                if (profileDto.Qualifications != null) doctor.Qualifications = profileDto.Qualifications;
                if (profileDto.Bio != null) doctor.Bio = profileDto.Bio;
                if (profileDto.ConsultationFee.HasValue) doctor.ConsultationFee = profileDto.ConsultationFee.Value;
                _unitOfWork.Doctors.Update(doctor);
            }
        }
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Profile updated for user {UserId}", userId);
        return await GetProfileAsync(userId);
    }

    // change user password
    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto passwordDto)
    {
        if (passwordDto.NewPassword != passwordDto.ConfirmNewPassword)
        {
            _logger.LogWarning("Password change failed for user {UserId}: passwords do not match", userId);
            return false;
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Password change failed: user {UserId} not found", userId);
            return false;
        }

        if (!VerifyPassword(passwordDto.CurrentPassword, user.PasswordHash))
        {
            _logger.LogWarning("Password change failed for user {UserId}: current password is incorrect", userId);
            return false;
        }

        user.PasswordHash = HashPassword(passwordDto.NewPassword);
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Password changed successfully for user {UserId}", userId);
        return true;
    }

    // hash password
    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    // verify password hash
    private bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}
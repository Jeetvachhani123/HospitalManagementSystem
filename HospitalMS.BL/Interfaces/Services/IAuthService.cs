using HospitalMS.BL.DTOs.Auth;

namespace HospitalMS.BL.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);

    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);

    Task<bool> ValidateTokenAsync(string token);

    Task<UserProfileDto?> GetProfileAsync(int userId);

    Task<UserProfileDto?> UpdateProfileAsync(int userId, ProfileUpdateDto profileDto);

    Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto passwordDto);
}
using HospitalMS.BL.Common;
using HospitalMS.BL.DTOs.Auth;
using HospitalMS.BL.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        if (result == null)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.ErrorResponse(Constants.Messages.LoginFailed));
        }
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, Constants.Messages.LoginSuccess));
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);
        if (result == null)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse(Constants.Messages.EmailAlreadyExists));
        }
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, Constants.Messages.RegistrationSuccess));
    }

    [HttpPost("validate")]
    public async Task<ActionResult<ApiResponse<bool>>> ValidateToken([FromBody] string token)
    {
        var isValid = await _authService.ValidateTokenAsync(token);
        return Ok(ApiResponse<bool>.SuccessResponse(isValid));
    }
}
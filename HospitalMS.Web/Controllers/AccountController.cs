using HospitalMS.BL.DTOs.Auth;
using HospitalMS.BL.Interfaces.Services;
using HospitalMS.Models.Enums;
using HospitalMS.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalMS.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly ILogger<AccountController> _logger;
    public AccountController(IAuthService authService, ILogger<AccountController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    // get current user id
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
       
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }

    // show login form
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
       
        return View();
    }

    // process login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto loginDto, string? returnUrl = null)
    {
        if (!ModelState.IsValid) 
            return View(loginDto);
        
        var result = await _authService.LoginAsync(loginDto);
        if (result == null)
        {
            ModelState.AddModelError("", "Invalid login attempt.");
            return View(loginDto);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, result.UserId.ToString()),
            new Claim(ClaimTypes.Email, result.Email),
            new Claim(ClaimTypes.Name, $"{result.FirstName} {result.LastName}"),
            new Claim(ClaimTypes.Role, result.Role)
        };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties { IsPersistent = true };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        
        if (result.Role == "Admin") 
            return RedirectToAction("Dashboard", "Admin");
        
        if (result.Role == "Doctor") 
            return RedirectToAction("Dashboard", "Doctor");
        
        if (result.Role == "Patient") 
            return RedirectToAction("Dashboard", "Patient");
       
        return RedirectToAction("Index", "Home");
    }

    // show register form
    [HttpGet]
    public IActionResult Register() => View();

    // process registration
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) 
            return View(model);
       
        var registerDto = new RegisterDto { Email = model.Email, Password = model.Password, FirstName = model.FirstName, LastName = model.LastName, PhoneNumber = model.PhoneNumber, Role = model.Role, DateOfBirth = model.DateOfBirth, Gender = model.Gender, BloodGroup = model.BloodGroup };
        var result = await _authService.RegisterAsync(registerDto);
        if (result == null)
        {
            ModelState.AddModelError("", "Registration failed. Email might be in use.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, result.UserId.ToString()),
            new Claim(ClaimTypes.Email, result.Email),
            new Claim(ClaimTypes.Name, $"{result.FirstName} {result.LastName}"),
            new Claim(ClaimTypes.Role, result.Role)
        };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties { IsPersistent = true };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity), authProperties);
       
        return RedirectToAction("Dashboard", "Patient");
    }

    // logout user
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    // access denied page
    [HttpGet]
    public IActionResult AccessDenied() => View();

    // view profile
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var userId = GetCurrentUserId();
        var profile = await _authService.GetProfileAsync(userId);
        if (profile == null)
        {
            return NotFound();
        }

        var model = new ProfileViewModel
        {
            Id = profile.Id,
            Email = profile.Email,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            FullName = profile.FullName,
            PhoneNumber = profile.PhoneNumber,
            Role = profile.Role,
            LastLoginAt = profile.LastLoginAt,
            CreatedAt = profile.CreatedAt
        };
       
        return View(model);
    }

    // show edit profile form
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> EditProfile()
    {
        var userId = GetCurrentUserId();
        var profile = await _authService.GetProfileAsync(userId);
        if (profile == null)
        {
            return NotFound();
        }

        var model = new ProfileEditViewModel
        {
            Id = profile.Id,
            Email = profile.Email,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            PhoneNumber = profile.PhoneNumber,
            DateOfBirth = profile.DateOfBirth,
            Gender = profile.Gender,
            BloodGroup = profile.BloodGroup,
            Street = profile.Street,
            City = profile.City,
            State = profile.State,
            ZipCode = profile.ZipCode,
            EmergencyContact = profile.EmergencyContact,
            MedicalHistory = profile.MedicalHistory,
            Allergies = profile.Allergies,
            Specialization = profile.Specialization,
            LicenseNumber = profile.LicenseNumber,
            YearsOfExperience = profile.YearsOfExperience,
            Qualifications = profile.Qualifications,
            Bio = profile.Bio,
            ConsultationFee = profile.ConsultationFee
        };

        return View(model);
    }

    // save profile edits
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(ProfileEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var userId = GetCurrentUserId();
        var updateDto = new ProfileUpdateDto
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber,
            DateOfBirth = model.DateOfBirth,
            Gender = model.Gender,
            BloodGroup = model.BloodGroup,
            Street = model.Street,
            City = model.City,
            State = model.State,
            ZipCode = model.ZipCode,
            EmergencyContact = model.EmergencyContact,
            MedicalHistory = model.MedicalHistory,
            Allergies = model.Allergies,
            Specialization = model.Specialization,
            LicenseNumber = model.LicenseNumber,
            YearsOfExperience = model.YearsOfExperience,
            Qualifications = model.Qualifications,
            Bio = model.Bio,
            ConsultationFee = model.ConsultationFee
        };
        var result = await _authService.UpdateProfileAsync(userId, updateDto);
        if (result == null)
        {
            ModelState.AddModelError("", "Failed to update profile.");
            return View(model);
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, result.Id.ToString()),
            new Claim(ClaimTypes.Email, result.Email),
            new Claim(ClaimTypes.Name, result.FullName),
            new Claim(ClaimTypes.Role, result.Role)
        };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties { IsPersistent = true };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity), authProperties);
        TempData["SuccessMessage"] = "Your profile has been updated successfully.";
       
        return RedirectToAction(nameof(Profile));
    }

    // show change password form
    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    // process password change
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = GetCurrentUserId();
        var passwordDto = new ChangePasswordDto
        {
            CurrentPassword = model.CurrentPassword,
            NewPassword = model.NewPassword,
            ConfirmNewPassword = model.ConfirmNewPassword
        };
        var success = await _authService.ChangePasswordAsync(userId, passwordDto);
        if (!success)
        {
            ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
            return View(model);
        }
        TempData["SuccessMessage"] = "Your password has been changed successfully.";
        
        return RedirectToAction(nameof(Profile));
    }
}
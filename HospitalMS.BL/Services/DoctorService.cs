using AutoMapper;
using HospitalMS.BL.DTOs.Doctor;
using HospitalMS.BL.Interfaces;
using HospitalMS.BL.Interfaces.Services;
using HospitalMS.Models.Entities;
using HospitalMS.Models.Enums;
using System.Security.Cryptography;
using System.Text;

namespace HospitalMS.BL.Services;

public class DoctorService : IDoctorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public DoctorService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // get doctor by id
    public async Task<DoctorResponseDto?> GetByIdAsync(int id)
    {
        var doctor = await _unitOfWork.Doctors.GetByIdAsync(id);
        return doctor == null ? null : MapToDoctorResponse(doctor);
    }

    // get doctor by user id
    public async Task<DoctorResponseDto?> GetByUserIdAsync(int userId)
    {
        var doctor = await _unitOfWork.Doctors.GetByUserIdAsync(userId);
        return doctor == null ? null : MapToDoctorResponse(doctor);
    }

    // get all doctors
    public async Task<IEnumerable<DoctorResponseDto>> GetAllAsync(int page = 1, int pageSize = 100)
    {
        var doctors = await _unitOfWork.Doctors.GetAllAsync(page, pageSize);
        return doctors.Select(MapToDoctorResponse);
    }

    // get available doctors
    public async Task<IEnumerable<DoctorResponseDto>> GetAvailableDoctorsAsync(int page = 1, int pageSize = 100)
    {
        var doctors = await _unitOfWork.Doctors.GetAvailableDoctorsAsync(page, pageSize);
        return doctors.Select(MapToDoctorResponse);
    }

    // get by specialization
    public async Task<IEnumerable<DoctorResponseDto>> GetBySpecializationAsync(string specialization)
    {
        var doctors = await _unitOfWork.Doctors.GetBySpecializationAsync(specialization);
        return doctors.Select(MapToDoctorResponse);
    }

    // create new doctor
    public async Task<DoctorResponseDto?> CreateAsync(DoctorCreateDto doctorDto)
    {
        if (await _unitOfWork.Users.EmailExistsAsync(doctorDto.Email)) return null;
        if (await _unitOfWork.Doctors.LicenseNumberExistsAsync(doctorDto.LicenseNumber)) return null;
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var user = new User { Email = doctorDto.Email, PasswordHash = HashPassword(doctorDto.Password), FirstName = doctorDto.FirstName, LastName = doctorDto.LastName, PhoneNumber = doctorDto.PhoneNumber, Role = UserRole.Doctor, IsActive = true };
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            var doctor = new Doctor { UserId = user.Id, Specialization = doctorDto.Specialization, LicenseNumber = doctorDto.LicenseNumber, YearsOfExperience = doctorDto.YearsOfExperience, Qualifications = doctorDto.Qualifications, Bio = doctorDto.Bio, ConsultationFee = doctorDto.ConsultationFee, IsAvailable = true };
            await _unitOfWork.Doctors.AddAsync(doctor);
            await _unitOfWork.SaveChangesAsync();
            await CreateDefaultWorkingHoursAsync(doctor.Id);
            doctor = await _unitOfWork.Doctors.GetByIdAsync(doctor.Id);
            return doctor == null ? null : MapToDoctorResponse(doctor);
        });
    }

    // create default working hours
    private async Task CreateDefaultWorkingHoursAsync(int doctorId)
    {
        var defaultWorkingHours = new List<DoctorWorkingHours>
        {
            new DoctorWorkingHours { DoctorId = doctorId, DayOfWeek = 0, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, IsWorkingDay = false },
            new DoctorWorkingHours { DoctorId = doctorId, DayOfWeek = 1, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), IsWorkingDay = true },
            new DoctorWorkingHours { DoctorId = doctorId, DayOfWeek = 2, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), IsWorkingDay = true },
            new DoctorWorkingHours { DoctorId = doctorId, DayOfWeek = 3, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), IsWorkingDay = true },
            new DoctorWorkingHours { DoctorId = doctorId, DayOfWeek = 4, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), IsWorkingDay = true },
            new DoctorWorkingHours { DoctorId = doctorId, DayOfWeek = 5, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), IsWorkingDay = true },
            new DoctorWorkingHours { DoctorId = doctorId, DayOfWeek = 6, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(14, 0, 0), IsWorkingDay = true },
        };
        foreach (var workingHour in defaultWorkingHours)
        {
            await _unitOfWork.DoctorWorkingHours.AddAsync(workingHour);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    // update doctor
    public async Task<DoctorResponseDto?> UpdateAsync(int id, DoctorUpdateDto doctorDto)
    {
        var doctor = await _unitOfWork.Doctors.GetByIdAsync(id);
        if (doctor == null) return null;
        if (doctorDto.PhoneNumber != null) doctor.User.PhoneNumber = doctorDto.PhoneNumber;
        if (doctorDto.Specialization != null) doctor.Specialization = doctorDto.Specialization;
        if (doctorDto.YearsOfExperience.HasValue) doctor.YearsOfExperience = doctorDto.YearsOfExperience.Value;
        if (doctorDto.Qualifications != null) doctor.Qualifications = doctorDto.Qualifications;
        if (doctorDto.Bio != null) doctor.Bio = doctorDto.Bio;
        if (doctorDto.ConsultationFee.HasValue) doctor.ConsultationFee = doctorDto.ConsultationFee.Value;
        if (doctorDto.IsAvailable.HasValue) doctor.IsAvailable = doctorDto.IsAvailable.Value;
        _unitOfWork.Doctors.Update(doctor);
        await _unitOfWork.SaveChangesAsync();
        return MapToDoctorResponse(doctor);
    }

    // delete doctor
    public async Task<bool> DeleteAsync(int id)
    {
        var doctor = await _unitOfWork.Doctors.GetByIdAsync(id);
        if (doctor == null) return false;
        _unitOfWork.Doctors.Delete(doctor);
        _unitOfWork.Users.Delete(doctor.User);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    // search doctors
    public async Task<(IEnumerable<DoctorResponseDto> Items, int TotalCount)> SearchAsync(string? searchTerm, int page, int pageSize)
    {
        var result = await _unitOfWork.Doctors.SearchAsync(searchTerm, page, pageSize);
        return (result.Items.Select(MapToDoctorResponse), result.TotalCount);
    }

    // map to response dto
    private DoctorResponseDto MapToDoctorResponse(Doctor doctor)
    {
        return new DoctorResponseDto
        {
            Id = doctor.Id,
            UserId = doctor.UserId,
            Email = doctor.User.Email,
            FirstName = doctor.User.FirstName,
            LastName = doctor.User.LastName,
            FullName = doctor.User.GetFullName(),
            PhoneNumber = doctor.User.PhoneNumber,
            Specialization = doctor.Specialization,
            LicenseNumber = doctor.LicenseNumber,
            YearsOfExperience = doctor.YearsOfExperience,
            Qualifications = doctor.Qualifications,
            Bio = doctor.Bio,
            ConsultationFee = doctor.ConsultationFee,
            IsAvailable = doctor.IsAvailable,
            CreatedAt = doctor.CreatedAt,
            DepartmentId = doctor.DepartmentId,
            DepartmentName = doctor.Department?.Name,
            CreatedBy = doctor.CreatedBy,
            UpdatedBy = doctor.UpdatedBy
        };
    }

    // hash password
    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }
}
using HospitalMS.BL.DTOs.Doctor;

namespace HospitalMS.BL.Interfaces.Services;

public interface IDoctorService
{
    Task<DoctorResponseDto?> GetByIdAsync(int id);

    Task<DoctorResponseDto?> GetByUserIdAsync(int userId);

    Task<IEnumerable<DoctorResponseDto>> GetAllAsync(int page = 1, int pageSize = 100);

    Task<IEnumerable<DoctorResponseDto>> GetAvailableDoctorsAsync(int page = 1, int pageSize = 100);

    Task<IEnumerable<DoctorResponseDto>> GetBySpecializationAsync(string specialization);

    Task<DoctorResponseDto?> CreateAsync(DoctorCreateDto doctorDto);

    Task<DoctorResponseDto?> UpdateAsync(int id, DoctorUpdateDto doctorDto);

    Task<bool> DeleteAsync(int id);

    Task<(IEnumerable<DoctorResponseDto> Items, int TotalCount)> SearchAsync(string? searchTerm, int page, int pageSize);
}
using HospitalMS.BL.DTOs.Patient;

namespace HospitalMS.BL.Interfaces.Services;

public interface IPatientService
{
    Task<PatientResponseDto?> GetByIdAsync(int id);

    Task<PatientResponseDto?> GetByUserIdAsync(int userId);

    Task<IEnumerable<PatientResponseDto>> GetAllAsync(int page = 1, int pageSize = 100);

    Task<PatientResponseDto?> CreateAsync(PatientCreateDto patientDto);

    Task<PatientResponseDto?> UpdateAsync(int id, PatientUpdateDto patientDto);

    Task<bool> DeleteAsync(int id);

    Task<(IEnumerable<PatientResponseDto> Items, int TotalCount)> SearchAsync(string? searchTerm, int page, int pageSize);
}
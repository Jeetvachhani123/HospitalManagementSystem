using HospitalMS.BL.DTOs.MedicalRecord;

namespace HospitalMS.BL.Interfaces.Services;

public interface IMedicalRecordService
{
    Task<IEnumerable<MedicalRecordDto>> GetByPatientIdAsync(int patientId);

    Task<IEnumerable<MedicalRecordDto>> GetByDoctorIdAsync(int doctorId);

    Task<MedicalRecordDto?> GetByIdAsync(int id);

    Task<MedicalRecordDto?> CreateAsync(MedicalRecordCreateDto dto);

    Task<bool> DeleteAsync(int id, int currentUserId);
}
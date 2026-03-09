using HospitalMS.Models.Entities;

namespace HospitalMS.BL.Interfaces.Repositories;

public interface IMedicalRecordRepository
{
    Task<MedicalRecord?> GetByIdAsync(int id);

    Task<IEnumerable<MedicalRecord>> GetByPatientIdAsync(int patientId);

    Task<IEnumerable<MedicalRecord>> GetByDoctorIdAsync(int doctorId);

    Task AddAsync(MedicalRecord medicalRecord);

    void Update(MedicalRecord medicalRecord);

    void Delete(MedicalRecord medicalRecord);
}
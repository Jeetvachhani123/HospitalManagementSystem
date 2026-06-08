using HospitalMS.DATA.Context;
using HospitalMS.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.DATA.Repositories;

public interface IMedicalRecordRepository
{
    Task<MedicalRecord?> GetByIdAsync(int id);
    Task<IEnumerable<MedicalRecord>> GetByPatientIdAsync(int patientId);
    Task<IEnumerable<MedicalRecord>> GetByDoctorIdAsync(int doctorId);
    Task AddAsync(MedicalRecord medicalRecord);
    void Update(MedicalRecord medicalRecord);
    void Delete(MedicalRecord medicalRecord);
}

public class MedicalRecordRepository : IMedicalRecordRepository
{
    private readonly HospitalDbContext _context;
    public MedicalRecordRepository(HospitalDbContext context)
    {
        _context = context;
    }

    // get record by id
    public async Task<MedicalRecord?> GetByIdAsync(int id)
    {
        return await _context.MedicalRecords
            .Include(m => m.Patient)
            .ThenInclude(p => p!.User)
            .Include(m => m.Doctor)
            .ThenInclude(d => d!.User)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    // get records by patient
    public async Task<IEnumerable<MedicalRecord>> GetByPatientIdAsync(int patientId)
    {
        return await _context.MedicalRecords
            .Include(m => m.Doctor)
            .ThenInclude(d => d!.User)
            .Where(m => m.PatientId == patientId)
            .OrderByDescending(m => m.RecordDate)
            .ToListAsync();
    }

    // get records by doctor
    public async Task<IEnumerable<MedicalRecord>> GetByDoctorIdAsync(int doctorId)
    {
        return await _context.MedicalRecords
            .Include(m => m.Patient)
            .ThenInclude(p => p.User)
            .Where(m => m.DoctorId == doctorId)
            .OrderByDescending(m => m.RecordDate)
            .ToListAsync();
    }

    // add medical record
    public async Task AddAsync(MedicalRecord medicalRecord)
    {
        await _context.MedicalRecords.AddAsync(medicalRecord);
    }

    // update medical record
    public void Update(MedicalRecord medicalRecord)
    {
        _context.MedicalRecords.Update(medicalRecord);
    }

    // soft delete record
    public void Delete(MedicalRecord medicalRecord)
    {
        medicalRecord.IsDeleted = true;
        medicalRecord.DeletedAt = DateTime.UtcNow;
        _context.MedicalRecords.Update(medicalRecord);
    }
}
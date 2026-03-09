using AutoMapper;
using HospitalMS.BL.DTOs.MedicalRecord;
using HospitalMS.BL.Interfaces;
using HospitalMS.BL.Interfaces.Services;
using HospitalMS.Models.Entities;
using HospitalMS.Models.Enums;
using Microsoft.Extensions.Logging;

namespace HospitalMS.BL.Services;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<MedicalRecordService> _logger;
    public MedicalRecordService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<MedicalRecordService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // get record by id
    public async Task<MedicalRecordDto?> GetByIdAsync(int id)
    {
        var record = await _unitOfWork.MedicalRecords.GetByIdAsync(id);
        return _mapper.Map<MedicalRecordDto>(record);
    }

    // get records by patient
    public async Task<IEnumerable<MedicalRecordDto>> GetByPatientIdAsync(int patientId)
    {
        var records = await _unitOfWork.MedicalRecords.GetByPatientIdAsync(patientId);
        return _mapper.Map<IEnumerable<MedicalRecordDto>>(records);
    }

    // get records by doctor
    public async Task<IEnumerable<MedicalRecordDto>> GetByDoctorIdAsync(int doctorId)
    {
        var records = await _unitOfWork.MedicalRecords.GetByDoctorIdAsync(doctorId);
        return _mapper.Map<IEnumerable<MedicalRecordDto>>(records);
    }

    // create medical record
    public async Task<MedicalRecordDto?> CreateAsync(MedicalRecordCreateDto dto)
    {
        try
        {
            var record = _mapper.Map<MedicalRecord>(dto);
            await _unitOfWork.MedicalRecords.AddAsync(record);
            await _unitOfWork.SaveChangesAsync();
            var createdRecord = await _unitOfWork.MedicalRecords.GetByIdAsync(record.Id);
            return _mapper.Map<MedicalRecordDto>(createdRecord);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating medical record");
            return null;
        }
    }

    // delete medical record
    public async Task<bool> DeleteAsync(int id, int currentUserId)
    {
        try
        {
            var record = await _unitOfWork.MedicalRecords.GetByIdAsync(id);
            if (record == null) return false;
            var user = await _unitOfWork.Users.GetByIdAsync(currentUserId);
            if (user == null) return false;
            if (user.Role == UserRole.Doctor)
            {
                var doctor = await _unitOfWork.Doctors.GetByUserIdAsync(currentUserId);
                if (doctor == null || record.DoctorId != doctor.Id)
                {
                    _logger.LogWarning($"Doctor {currentUserId} attempted to delete record {id} owned by {record.DoctorId}");
                    return false;
                }
            }
            else if (user.Role != UserRole.Admin)
            {
                return false;
            }
            _unitOfWork.MedicalRecords.Delete(record);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting medical record {id}");
            return false;
        }
    }
}
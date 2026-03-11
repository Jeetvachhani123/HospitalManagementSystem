using AutoMapper;
using HospitalMS.BL.DTOs.Patient;
using HospitalMS.BL.Interfaces;
using HospitalMS.BL.Interfaces.Services;
using HospitalMS.Models.Entities;
using HospitalMS.Models.Enums;
using System.Security.Cryptography;
using System.Text;

namespace HospitalMS.BL.Services;

public class PatientService : IPatientService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public PatientService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // get patient by id
    public async Task<PatientResponseDto?> GetByIdAsync(int id)
    {
        var patient = await _unitOfWork.Patients.GetByIdAsync(id);
        return patient == null ? null : MapToPatientResponse(patient);
    }

    // get patient by user id
    public async Task<PatientResponseDto?> GetByUserIdAsync(int userId)
    {
        var patient = await _unitOfWork.Patients.GetByUserIdAsync(userId);
        return patient == null ? null : MapToPatientResponse(patient);
    }

    // get all patients
    public async Task<IEnumerable<PatientResponseDto>> GetAllAsync(int page = 1, int pageSize = 100)
    {
        var patients = await _unitOfWork.Patients.GetAllAsync(page, pageSize);
        return patients.Select(MapToPatientResponse);
    }

    // create new patient
    public async Task<PatientResponseDto?> CreateAsync(PatientCreateDto patientDto)
    {
        if (await _unitOfWork.Users.EmailExistsAsync(patientDto.Email))
        {
            return null;
        }
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var user = new User { Email = patientDto.Email, PasswordHash = HashPassword(patientDto.Password), FirstName = patientDto.FirstName, LastName = patientDto.LastName, PhoneNumber = patientDto.PhoneNumber, Role = UserRole.Patient, IsActive = true };
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            var patient = new Patient { UserId = user.Id, DateOfBirth = patientDto.DateOfBirth, BloodGroup = patientDto.BloodGroup, Gender = patientDto.Gender, EmergencyContact = patientDto.EmergencyContact };
            await _unitOfWork.Patients.AddAsync(patient);
            await _unitOfWork.SaveChangesAsync();
            patient = await _unitOfWork.Patients.GetByIdAsync(patient.Id);
            return patient == null ? null : MapToPatientResponse(patient);
        });
    }

    // delete patient
    public async Task<bool> DeleteAsync(int id)
    {
        var patient = await _unitOfWork.Patients.GetByIdAsync(id);
        if (patient == null)
        {
            return false;
        }
        _unitOfWork.Patients.Delete(patient);
        _unitOfWork.Users.Delete(patient.User);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    // update patient
    public async Task<PatientResponseDto?> UpdateAsync(int id, PatientUpdateDto patientDto)
    {
        var patient = await _unitOfWork.Patients.GetByIdAsync(id);
        if (patient == null) return null;
        if (patientDto.PhoneNumber != null) patient.User.PhoneNumber = patientDto.PhoneNumber;
        if (patientDto.BloodGroup != null) patient.BloodGroup = patientDto.BloodGroup;
        if (patientDto.Gender != null) patient.Gender = patientDto.Gender;
        if (patientDto.EmergencyContact != null) patient.EmergencyContact = patientDto.EmergencyContact;
        if (patientDto.MedicalHistory != null) patient.MedicalHistory = patientDto.MedicalHistory;
        if (patientDto.Allergies != null) patient.Allergies = patientDto.Allergies;
        _unitOfWork.Patients.Update(patient);
        await _unitOfWork.SaveChangesAsync();
        return MapToPatientResponse(patient);
    }

    // search patients
    public async Task<(IEnumerable<PatientResponseDto> Items, int TotalCount)> SearchAsync(string? searchTerm, int page, int pageSize)
    {
        var result = await _unitOfWork.Patients.SearchAsync(searchTerm, page, pageSize);
        return (result.Items.Select(MapToPatientResponse), result.TotalCount);
    }

    // map to response dto
    private PatientResponseDto MapToPatientResponse(Patient patient)
    {
        return new PatientResponseDto
        {
            Id = patient.Id,
            UserId = patient.UserId,
            Email = patient.User.Email,
            FirstName = patient.User.FirstName,
            LastName = patient.User.LastName,
            FullName = patient.User.GetFullName(),
            PhoneNumber = patient.User.PhoneNumber,
            DateOfBirth = patient.DateOfBirth,
            Age = patient.GetAge(),
            BloodGroup = patient.BloodGroup,
            Gender = patient.Gender,
            EmergencyContact = patient.EmergencyContact,
            MedicalHistory = patient.MedicalHistory,
            Allergies = patient.Allergies,
            CreatedAt = patient.CreatedAt,
            CreatedBy = patient.CreatedBy,
            UpdatedBy = patient.UpdatedBy
        };
    }

    // hash password
    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }
}
using AutoMapper;
using HospitalMS.BL.DTOs.MedicalRecord;
using HospitalMS.Models.Entities;

namespace HospitalMS.BL.Mappings;

public class MedicalRecordProfile : Profile
{
    public MedicalRecordProfile()
    {
        CreateMap<MedicalRecord, MedicalRecordDto>()
            .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient.User.FirstName + " " + src.Patient.User.LastName))
            .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor != null ? src.Doctor.User.FirstName + " " + src.Doctor.User.LastName : string.Empty));
        CreateMap<MedicalRecordCreateDto, MedicalRecord>();
    }
}
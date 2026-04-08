using AutoMapper;
using HospitalMS.BL.DTOs.Appointment;
using HospitalMS.BL.DTOs.Auth;
using HospitalMS.BL.DTOs.Doctor;
using HospitalMS.BL.DTOs.Patient;
using HospitalMS.Models.Entities;

namespace HospitalMS.BL.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<RegisterDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
        
        CreateMap<DoctorCreateDto, Doctor>();
       
        CreateMap<Doctor, DoctorResponseDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.GetFullName()))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null));
        
        CreateMap<PatientCreateDto, Patient>();
       
        CreateMap<Patient, PatientResponseDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.GetFullName()))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.GetAge()));
        
        CreateMap<AppointmentCreateDto, Appointment>();
        
        CreateMap<Appointment, AppointmentResponseDto>()
            .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient.User.GetFullName()))
            .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor.User.GetFullName()))
            .ForMember(dest => dest.DoctorSpecialization, opt => opt.MapFrom(src => src.Doctor.Specialization))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.StatusEnum, opt => opt.MapFrom(src => src.Status));
    }
}
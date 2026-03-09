using AutoMapper;
using HospitalMS.BL.DTOs.Department;
using HospitalMS.Models.Entities;

namespace HospitalMS.BL.Mappings;

public class DepartmentProfile : Profile
{
    public DepartmentProfile()
    {
        CreateMap<Department, DepartmentDto>()
            .ForMember(dest => dest.DoctorCount, opt => opt.MapFrom(src => src.Doctors != null ? src.Doctors.Count : 0));
        CreateMap<CreateDepartmentDto, Department>();
        CreateMap<UpdateDepartmentDto, Department>();
    }
}
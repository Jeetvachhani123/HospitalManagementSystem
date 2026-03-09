using HospitalMS.BL.DTOs.Doctor;

namespace HospitalMS.BL.Interfaces.Services;

public interface IWorkingHoursService
{
    Task<List<WorkingHoursDto>> GetWorkingHoursAsync(int doctorId);

    Task UpdateWorkingHoursAsync(int doctorId, List<WorkingHoursDto> hours);
}
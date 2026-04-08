using HospitalMS.BL.DTOs.Doctor;
using HospitalMS.BL.Interfaces;
using HospitalMS.BL.Interfaces.Services;
using HospitalMS.Models.Entities;
using Microsoft.Extensions.Logging;

namespace HospitalMS.BL.Services;

public class WorkingHoursService : IWorkingHoursService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WorkingHoursService> _logger;
    public WorkingHoursService(IUnitOfWork unitOfWork, ILogger<WorkingHoursService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // get working hours
    public async Task<List<WorkingHoursDto>> GetWorkingHoursAsync(int doctorId)
    {
        var hours = await _unitOfWork.DoctorWorkingHours.GetByDoctorIdAsync(doctorId);
        if (!hours.Any())
        {
            for (int i = 0; i <= 6; i++)
            {
                var newHour = new DoctorWorkingHours { DoctorId = doctorId, DayOfWeek = i, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(21, 0, 0), IsWorkingDay = (i != 0 && i != 6) };
                await _unitOfWork.DoctorWorkingHours.AddAsync(newHour);
            }
            await _unitOfWork.SaveChangesAsync();
            hours = await _unitOfWork.DoctorWorkingHours.GetByDoctorIdAsync(doctorId);
        }
       
        return hours.OrderBy(h => h.DayOfWeek).Select(h => new WorkingHoursDto
        {
            Id = h.Id,
            DoctorId = h.DoctorId,
            DayOfWeek = h.DayOfWeek,
            IsWorkingDay = h.IsWorkingDay,
            StartTime = h.StartTime,
            EndTime = h.EndTime
        }).ToList();
    }

    // update working hours
    public async Task UpdateWorkingHoursAsync(int doctorId, List<WorkingHoursDto> hoursDto)
    {
        var existingHours = await _unitOfWork.DoctorWorkingHours.GetByDoctorIdAsync(doctorId);
        foreach (var dto in hoursDto)
        {
            var existing = existingHours.FirstOrDefault(h => h.DayOfWeek == dto.DayOfWeek);
            if (existing != null)
            {
                existing.IsWorkingDay = dto.IsWorkingDay;
                existing.StartTime = dto.StartTime;
                existing.EndTime = dto.EndTime;
                _unitOfWork.DoctorWorkingHours.Update(existing);
            }
            else
            {
                var newHour = new DoctorWorkingHours { DoctorId = doctorId, DayOfWeek = dto.DayOfWeek, IsWorkingDay = dto.IsWorkingDay, StartTime = dto.StartTime, EndTime = dto.EndTime };
                await _unitOfWork.DoctorWorkingHours.AddAsync(newHour);
            }
        }
        await _unitOfWork.SaveChangesAsync();
    }
}
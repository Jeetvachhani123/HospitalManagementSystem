using HospitalMS.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalMS.DATA.Configurations;

public class DoctorWorkingHoursConfiguration : IEntityTypeConfiguration<DoctorWorkingHours>
{
    public void Configure(EntityTypeBuilder<DoctorWorkingHours> builder)
    {
        builder.ToTable("DoctorWorkingHours");
       
        builder.HasKey(dw => dw.Id);
       
        builder.Property(dw => dw.DayOfWeek)
            .IsRequired();
       
        builder.Property(dw => dw.StartTime)
            .IsRequired();
      
        builder.Property(dw => dw.EndTime)
            .IsRequired();
       
        builder.Property(dw => dw.IsWorkingDay)
            .HasDefaultValue(true);
      
        builder.HasOne(dw => dw.Doctor)
            .WithMany(d => d.WorkingHours)
            .HasForeignKey(dw => dw.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);
       
        builder.HasIndex(dw => dw.DoctorId);
       
        builder.HasIndex(dw => new { dw.DoctorId, dw.DayOfWeek }).IsUnique();
    }
}
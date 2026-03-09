using HospitalMS.Models.Entities;
using HospitalMS.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalMS.DATA.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.AppointmentDate)
            .IsRequired();
        builder.Property(a => a.StartTime)
            .IsRequired();
        builder.Property(a => a.EndTime)
            .IsRequired();
        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(a => a.ApprovalStatus)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(a => a.Reason)
            .HasMaxLength(500);
        builder.Property(a => a.Notes)
            .HasMaxLength(2000);
        builder.Property(a => a.Diagnosis)
            .HasMaxLength(2000);
        builder.Property(a => a.Prescription)
            .HasMaxLength(5000);
        builder.Property(a => a.RejectionReason)
            .HasMaxLength(500);
        builder.Property(a => a.ApprovedAt)
            .IsRequired(false);
        builder.Property(a => a.RescheduledAt)
            .IsRequired(false);
        builder.Property(a => a.RowVersion)
            .IsRowVersion();
        builder.HasIndex(a => a.AppointmentDate);
        builder.HasIndex(a => new { a.DoctorId, a.AppointmentDate });
        builder.HasIndex(a => new { a.PatientId, a.AppointmentDate });
        builder.HasIndex(a => a.ApprovalStatus);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.CreatedAt);
        builder.HasIndex(a => new { a.DoctorId, a.AppointmentDate, a.StartTime, a.EndTime, a.Status, a.ApprovalStatus })
            .HasFilter("[Status] NOT IN (2, 4) AND [ApprovalStatus] != 2") 
            .IsUnique();
    }
}
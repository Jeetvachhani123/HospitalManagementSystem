using HospitalMS.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalMS.DATA.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Specialization)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(d => d.LicenseNumber)
            .IsRequired()
            .HasMaxLength(100);
        builder.HasIndex(d => d.LicenseNumber)
            .IsUnique();
        builder.Property(d => d.Qualifications)
            .HasMaxLength(500);
        builder.Property(d => d.Bio)
            .HasMaxLength(2000);
        builder.Property(d => d.ConsultationFee)
            .HasPrecision(18, 2);
        builder.Property(d => d.RowVersion)
            .IsRowVersion();
        builder.HasMany(d => d.Appointments)
            .WithOne(a => a.Doctor)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
using HospitalMS.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalMS.DATA.Configurations;

public class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
{
    public void Configure(EntityTypeBuilder<MedicalRecord> builder)
    {
        builder.ToTable("MedicalRecords");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.RecordType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Diagnosis)
            .HasMaxLength(2000);

        builder.Property(m => m.Prescription)
            .HasMaxLength(2000);

        builder.Property(m => m.Notes)
            .HasMaxLength(2000);

        builder.Property(m => m.AttachmentPath)
            .HasMaxLength(500);

        builder.Property(m => m.RowVersion)
            .IsRowVersion();

        builder.HasIndex(m => m.PatientId);

        builder.HasIndex(m => m.DoctorId);

        builder.HasOne(m => m.Patient)
            .WithMany()
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Doctor)
            .WithMany()
            .HasForeignKey(m => m.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

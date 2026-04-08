using HospitalMS.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalMS.DATA.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");
       
        builder.HasKey(p => p.Id);
       
        builder.Property(p => p.BloodGroup)
            .HasMaxLength(10);
       
        builder.Property(p => p.Gender)
            .HasMaxLength(20);
       
        builder.Property(p => p.EmergencyContact)
            .HasMaxLength(20);
       
        builder.Property(p => p.MedicalHistory)
            .HasMaxLength(5000);
       
        builder.Property(p => p.Allergies)
            .HasMaxLength(1000);
       
        builder.Property(p => p.RowVersion)
            .IsRowVersion();
       
        builder.OwnsOne(p => p.Address, address =>
        {
            address.Property(a => a.Street).HasMaxLength(200);
            address.Property(a => a.City).HasMaxLength(100);
            address.Property(a => a.State).HasMaxLength(100);
            address.Property(a => a.Country).HasMaxLength(100);
            address.Property(a => a.ZipCode).HasMaxLength(20);
        });
       
        builder.HasMany(p => p.Appointments)
            .WithOne(a => a.Patient)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
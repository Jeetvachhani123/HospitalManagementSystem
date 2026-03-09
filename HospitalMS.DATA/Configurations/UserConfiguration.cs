using HospitalMS.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalMS.DATA.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);
        builder.HasIndex(u => u.Email)
            .IsUnique();
        builder.HasIndex(u => new { u.FirstName, u.LastName });
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);
        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<int>();
        builder.HasOne(u => u.Doctor)
            .WithOne(d => d.User)
            .HasForeignKey<Doctor>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(u => u.Patient)
            .WithOne(p => p.User)
            .HasForeignKey<Patient>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
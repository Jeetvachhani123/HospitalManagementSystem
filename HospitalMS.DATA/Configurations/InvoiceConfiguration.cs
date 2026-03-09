using HospitalMS.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalMS.DATA.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Amount)
            .IsRequired()
            .HasPrecision(18, 2);
        builder.Property(i => i.IssueDate)
            .IsRequired();
        builder.Property(i => i.DueDate)
            .IsRequired();
        builder.Property(i => i.IsPaid)
            .IsRequired();
        builder.Property(i => i.PaidAt)
            .IsRequired(false);
        builder.Property(i => i.PaymentMethod)
            .HasMaxLength(50);
        builder.HasIndex(i => i.AppointmentId)
            .IsUnique();
        builder.HasIndex(i => i.PatientId);
        builder.HasIndex(i => i.IssueDate);
        builder.HasIndex(i => i.DueDate);
        builder.HasIndex(i => new { i.PatientId, i.IsPaid });
        builder.HasOne(i => i.Patient)
            .WithMany()
            .HasForeignKey(i => i.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(i => i.Appointment)
            .WithMany()
            .HasForeignKey(i => i.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
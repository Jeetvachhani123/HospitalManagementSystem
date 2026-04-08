using HospitalMS.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalMS.DATA.Configurations;

public class AppointmentStatusHistoryConfiguration : IEntityTypeConfiguration<AppointmentStatusHistory>
{
    public void Configure(EntityTypeBuilder<AppointmentStatusHistory> builder)
    {
        builder.ToTable("AppointmentStatusHistories");
       
        builder.HasKey(ash => ash.Id);
       
        builder.Property(ash => ash.NewStatus)
            .IsRequired()
            .HasConversion<int>();
       
        builder.Property(ash => ash.PreviousStatus)
            .HasConversion<int?>();
        
        builder.Property(ash => ash.NewApprovalStatus)
            .HasConversion<int?>();
       
        builder.Property(ash => ash.PreviousApprovalStatus)
            .HasConversion<int?>();
       
        builder.Property(ash => ash.ChangedBy)
            .IsRequired()
            .HasMaxLength(256);
       
        builder.Property(ash => ash.ChangedAt)
            .IsRequired();
       
        builder.Property(ash => ash.ChangeReason)
            .HasMaxLength(500);
       
        builder.HasOne(ash => ash.Appointment)
            .WithMany()
            .HasForeignKey(ash => ash.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(ash => ash.AppointmentId);
       
        builder.HasIndex(ash => ash.ChangedAt);
    }
}
using HospitalMS.Models.Base;
using HospitalMS.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace HospitalMS.DATA.Context;

public class HospitalDbContext : DbContext
{
    public HospitalDbContext(DbContextOptions<HospitalDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Doctor> Doctors { get; set; }

    public DbSet<Patient> Patients { get; set; }

    public DbSet<Appointment> Appointments { get; set; }

    public DbSet<DoctorWorkingHours> DoctorWorkingHours { get; set; }

    public DbSet<AppointmentStatusHistory> AppointmentStatusHistories { get; set; }

    public DbSet<Invoice> Invoices { get; set; }

    public DbSet<MedicalRecord> MedicalRecords { get; set; }

    public DbSet<Department> Departments { get; set; }

    // configure entity models
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Doctor>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<Patient>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Appointment>().HasQueryFilter(a => !a.IsDeleted);
        modelBuilder.Entity<DoctorWorkingHours>().HasQueryFilter(dw => !dw.IsDeleted);
        modelBuilder.Entity<Invoice>().HasQueryFilter(i => !i.IsDeleted);
        modelBuilder.Entity<MedicalRecord>().HasQueryFilter(m => !m.IsDeleted);
        modelBuilder.Entity<Department>().HasQueryFilter(d => !d.IsDeleted);
    }

    // save changes async
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    // save changes sync
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    // update audit timestamps and handle soft-delete
    private void UpdateAuditFields()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = null;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }

        // intercept hard-delete on AuditableEntity — convert to soft-delete
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = now;
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
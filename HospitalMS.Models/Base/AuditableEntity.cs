namespace HospitalMS.Models.Base;

public abstract class AuditableEntity : BaseEntity
{
    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }
}
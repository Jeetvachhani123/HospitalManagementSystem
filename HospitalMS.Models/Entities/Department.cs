using HospitalMS.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace HospitalMS.Models.Entities
{
    public class Department : AuditableEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
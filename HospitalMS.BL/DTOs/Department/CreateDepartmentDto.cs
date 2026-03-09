namespace HospitalMS.BL.DTOs.Department;

public class CreateDepartmentDto
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Location { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UpdateDepartmentDto : CreateDepartmentDto
{
    public int Id { get; set; }
}
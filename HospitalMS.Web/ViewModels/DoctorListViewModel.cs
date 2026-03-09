namespace HospitalMS.Web.ViewModels;

public class DoctorListViewModel
{
    public List<DoctorViewModel> Doctors { get; set; } = new();

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public string? SearchQuery { get; set; }

    public string? SelectedSpecialization { get; set; }

    public List<string> Specializations { get; set; } = new();

    public bool HasPreviousPage => CurrentPage > 1;

    public bool HasNextPage => CurrentPage < TotalPages;
}
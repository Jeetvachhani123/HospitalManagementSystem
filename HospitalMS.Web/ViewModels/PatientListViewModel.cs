namespace HospitalMS.Web.ViewModels;

public class PatientListViewModel
{
    public List<PatientViewModel> Patients { get; set; } = new();

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public string? SearchQuery { get; set; }

    public bool HasPreviousPage => CurrentPage > 1;

    public bool HasNextPage => CurrentPage < TotalPages;
}
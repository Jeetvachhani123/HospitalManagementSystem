namespace HospitalMS.Web.ViewModels;

public class AppointmentListViewModel
{
    public List<AppointmentViewModel> Appointments { get; set; } = new();

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public string? SearchQuery { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public bool HasPreviousPage => CurrentPage > 1;

    public bool HasNextPage => CurrentPage < TotalPages;
}
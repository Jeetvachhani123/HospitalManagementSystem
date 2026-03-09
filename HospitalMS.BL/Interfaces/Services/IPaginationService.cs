namespace HospitalMS.BL.Interfaces.Services;

public interface IPaginationService
{
    Task<PaginatedResult<T>> PaginateAsync<T>(IQueryable<T> query, int pageNumber, int pageSize) where T : class;

    Task<PaginatedResult<T>> PaginateAsync<T>(List<T> items, int pageNumber, int pageSize) where T : class;
}

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;
}
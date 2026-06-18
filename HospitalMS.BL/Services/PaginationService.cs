using HospitalMS.BL.Models;

namespace HospitalMS.BL.Services;

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

public class PaginationService
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;

    public static PagedResult<T> Create<T>(IEnumerable<T> source, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? DefaultPageSize : pageSize;
        pageSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;
        var totalCount = source.Count();
        var items = source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return PagedResult<T>.Create(items, pageNumber, pageSize, totalCount);
    }

    public static (int pageNumber, int pageSize) ValidatePageParameters(int pageNumber, int pageSize)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? DefaultPageSize : pageSize;
        pageSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;

        return (pageNumber, pageSize);
    }
}
using HospitalMS.BL.Models;

namespace HospitalMS.BL.Services;

public class PaginationService
{
    private const int DefaultPageSize = 10;

    private const int MaxPageSize = 100;

    // create paged result
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

    // validate page params
    public static (int pageNumber, int pageSize) ValidatePageParameters(int pageNumber, int pageSize)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? DefaultPageSize : pageSize;
        pageSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;
       
        return (pageNumber, pageSize);
    }
}
using System.Security.Claims;

namespace approvalworkflow.Services;

public interface IRepositoryService<TBy, TFor> 
                        where TBy : class
                        where TFor : class
{
    Task<List<TBy>> GetRecordsByUserAsync(ClaimsPrincipal user, Paginator<TBy>? paginator = null);
    Task<List<TFor>> GetRecordsForUserAsync(ClaimsPrincipal user);
    Task<OpResult> CreateRecordAsync(TBy newRecord);
    Task<OpResult> UpdateRecordAsync(TBy record);
    Task<OpResult> UpdateRecordAsync(TFor record);
    Task<bool> DeleteRecordAsync(int recordId);
}
public record OpResult(bool Success, EventId? ErrorEventId = null, object? Data = null);

public interface ILookupService<T> where T : class
{
    List<T> GetRecords();
    T GetRecord(int recordId);
}

public class Paginator<T> where T : class
{
    public int PageSize { get; set; }
    public int Page { get; set; }
    private int _pageCount;
    public int PageCount => _pageCount;

    private int _totalRecords;
    public int TotalRecords => _totalRecords;

    public int Start => ((Page - 1) * PageSize) + 1;
    public int End => Page * PageSize;

    public IQueryable<T> GetRecords(IQueryable<T> values)
    {
        _totalRecords = values.Count();
        if(PageSize == 0)
        {
            _pageCount = 1;
            return values;
        }
        else
        {
            _pageCount = (_totalRecords / PageSize) + 1;
            return values.Skip((Page-1) * PageSize).Take(PageSize);
        }
    }
}

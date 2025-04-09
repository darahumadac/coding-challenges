using System.Linq.Expressions;
using System.Security.Claims;

namespace approvalworkflow.Services;

public interface IRepositoryService<TBy, TFor> 
                        where TBy : class
                        where TFor : class
{
    Task<List<TBy>> GetRecordsByUserAsync(ClaimsPrincipal user, Paginated<TBy>? paginator = null, List<Expression<Func<TBy, bool>>>? filters = null);
    Task<List<TFor>> GetRecordsForUserAsync(ClaimsPrincipal user);
    Task<OpResult> CreateRecordAsync(TBy newRecord);
    Task<OpResult> UpdateRecordAsync(TBy record);
    Task<OpResult> UpdateRecordAsync(TFor record);
    Task<bool> DeleteRecordAsync(ClaimsPrincipal user, int recordId);
    Task<OpResult> GetRecordByUserAsync(ClaimsPrincipal user, int recordId);
}
public record OpResult(bool Success, EventId? ErrorEventId = null, object? Data = null);

public interface ILookupService<T> where T : class
{
    List<T> GetRecords();
    T GetRecord(int recordId);

    public T UKNOWN { get;}
}

public class Paginated<T> where T : class
{
    public int PageSize { get; set;}
    private int _page;
    public int Page 
    {
        get
        {
            return Math.Max(1, _page);
        }
        set
        {
            _page = Math.Max(1, _page);
        }
    }
    public int PageCount => PageSize > 0 ? (_totalRecords / PageSize) + 1 : 1;
        
    private int _totalRecords;
    public int TotalRecords => _totalRecords;

    public int Start => ((Page - 1) * PageSize) + 1;
    public int End => Page * PageSize;

    public IQueryable<T> GetRecords(IQueryable<T> values)
    {
        _totalRecords = values.Count();
        if(PageSize == 0)
        {
            return values;
        }
        else
        {
            return values.Skip((Page-1) * PageSize).Take(PageSize);
        }
    }
}

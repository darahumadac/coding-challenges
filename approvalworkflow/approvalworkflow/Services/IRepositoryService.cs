using System.Security.Claims;
using approvalworkflow.Database;
using approvalworkflow.Enums;
using approvalworkflow.Models;

namespace approvalworkflow.Services;

public interface IRepositoryService<TBy, TFor> 
                        where TBy : class
                        where TFor : class
{
    Task<List<TBy>> GetRecordsByUserAsync(ClaimsPrincipal user);
    Task<List<TFor>> GetRecordsForUserAsync(ClaimsPrincipal user);
    Task<OpResult> CreateRecordAsync(TBy newRecord);
    Task<OpResult> UpdateRecordAsync(TBy record);
    Task<OpResult> UpdateRecordAsync(TFor record);
    // Task<T> GetRecordAsync(int recordId);
    Task<bool> DeleteRecordAsync(int recordId);
}
public record OpResult(bool Success, EventId? ErrorEventId = null, object? Data = null);

public interface ILookupService<T> where T : class
{
    List<T> GetRecords();
    T GetRecord(int recordId);
}


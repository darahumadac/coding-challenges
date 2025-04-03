using System.Security.Claims;
using approvalworkflow.Database;
using approvalworkflow.Enums;
using approvalworkflow.Models;

namespace approvalworkflow.Services;

public interface IRepositoryService<T, TRequestCategory>
                where T : class
                where TRequestCategory : class
{
    Task<List<T>> GetRecordsByUserAsync(ClaimsPrincipal user);
    Task<List<T>> GetRecordsForUserAsync(ClaimsPrincipal user);
    Task<OpResult> CreateRecordAsync(T newRecord);

    Task<OpResult> UpdateRecordAsync(T record);

    List<TRequestCategory> RequestCategories();
    TRequestCategory GetRequestCategory(int categoryId);

    Task<bool> DeleteRecordAsync(int recordId);
}

public record OpResult(bool Success, string? errorCode = null);

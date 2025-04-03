using System.Security.Claims;
using approvalworkflow.Database;
using approvalworkflow.Models;

namespace approvalworkflow.Services;

public interface IRepositoryService<T, TRequestCategory>
                where T : class
                where TRequestCategory : class
{
    Task<List<T>> GetRecordsByUserAsync(ClaimsPrincipal user);
    Task<List<T>> GetRecordsForUserAsync(ClaimsPrincipal user);
    Task<bool> CreateRecordAsync(T newRecord);

    Task<bool> UpdateRecordAsync(T record);

    List<TRequestCategory> RequestCategories();
    TRequestCategory GetRequestCategory(int id);

}

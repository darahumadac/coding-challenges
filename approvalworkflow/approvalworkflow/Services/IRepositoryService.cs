using System.Security.Claims;
using approvalworkflow.Database;
using approvalworkflow.Models;

namespace approvalworkflow.Services;

public interface IRepositoryService<T> where T : class
{
    Task<List<T>> GetRecordsByUserAsync(ClaimsPrincipal user);
    Task<List<T>> GetRecordsForUserAsync(ClaimsPrincipal user);

}

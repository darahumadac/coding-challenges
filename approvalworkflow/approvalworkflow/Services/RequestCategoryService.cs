using approvalworkflow.Database;
using approvalworkflow.Models;

namespace approvalworkflow.Services;

public class RequestCategoryService : ILookupService<RequestCategory>
{
    private readonly AppDbContext _dbContext;
    public readonly RequestCategory UNKNOWN_CATEGORY = new();

    public RequestCategory UKNOWN => UNKNOWN_CATEGORY;

    public RequestCategoryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public RequestCategory GetRecord(int requestCategoryId)
    {
        return _dbContext.RequestCategories.Find(requestCategoryId) ?? UNKNOWN_CATEGORY;
    }

    public List<RequestCategory> GetRecords()
    {
        return _dbContext.RequestCategories.ToList();
    }
}
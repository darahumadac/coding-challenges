using System.Security.Claims;
using approvalworkflow.Database;
using approvalworkflow.Enums;
using approvalworkflow.Models;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace approvalworkflow.Services;

public class RequestService : IRepositoryService<UserRequest, RequestCategory>
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _dbContext;

    private readonly User UNKNOWN_USER = new();

    private readonly ILogger<RequestService> _logger;
    private readonly AppUserService _appUserService;
    private RequestCategory UNKNOWN_CATEGORY = new();

    public RequestService(UserManager<User> userManager, AppDbContext dbContext, ILogger<RequestService> logger, AppUserService appUserService)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _logger = logger;
        _appUserService = appUserService;
    }

    public async Task<List<UserRequest>> GetRecordsByUserAsync(ClaimsPrincipal user)
    {
        var currentUser = await _appUserService.AppUserAsync(user);
        return await _dbContext.UserRequests
                .Where(u => u.CreatedById == currentUser.Id)
                .Include(u => u.Type)
                .ToListAsync();
    }

    public async Task<List<UserRequest>> GetRecordsForUserAsync(ClaimsPrincipal user)
    {
        var currentUser = await _appUserService.AppUserAsync(user);
        if(!currentUser.Roles.Contains(AppRoles.Approver.ToString()))
        {
            return new List<UserRequest>();
        }


        return await _dbContext.RequestApprovals
                    .Where(a => a.ApproverId == currentUser.Id)
                    .Include(a => a.Request)
                        .ThenInclude(r => r.Type)
                    .Include(a => a.Request)
                        .ThenInclude(r => r.CreatedBy)
                    .Select(a => a.Request)
                    .ToListAsync();
        
        //LINQ Query syntax
        // var forApprovals = 
        //     from a in _dbContext.RequestApprovals
        //     where a.ApproverId == currentUser.Id
        //     join r in _dbContext.UserRequests
        //         on a.RequestId equals r.Id
        //     select r;

        // return await forApprovals.ToListAsync();      
                
        
    }

    public async Task<bool> CreateRecordAsync(UserRequest newRequest)
    {
        try
        {   
            var now = DateTime.UtcNow;
            newRequest.CreatedDate = now;
            newRequest.UpdatedDate = now;

            var createdBy = await _appUserService.AppUserAsync(newRequest.User);
            newRequest.CreatedById = createdBy.Id;

            var requestType = GetRequestCategory(newRequest.TypeId);
            if(requestType == UNKNOWN_CATEGORY)
            {
                //add message here                
                return false;
            }
            
            //get the approvers (supervisor tree)
            var approverList = await _appUserService.GetSupervisorTreeAsync(newRequest.CreatedById, requestType.RequiredApproverCount);
            if(approverList != null)
            {
                newRequest.Approvals = approverList.Select(id => new RequestApproval{ApproverId = id, Status = ApprovalStatus.Pending}).ToList();
            }
            
            var transaction = await _dbContext.Database.BeginTransactionAsync();
            _dbContext.UserRequests.Add(newRequest);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            _dbContext.Database.RollbackTransaction();
        }

        return false;

    }

    public async Task<bool> UpdateRecordAsync(UserRequest request)
    {
        //TODO: Update implementation. this is a stub
        return true;
    }

    public List<RequestCategory> RequestCategories()
    {
        return _dbContext.RequestCategories.ToList();
    }

    public RequestCategory GetRequestCategory(int id)
    {
        return _dbContext.RequestCategories.Find(id) ?? UNKNOWN_CATEGORY;
    }
}

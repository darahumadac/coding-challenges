using System.Security.Claims;
using approvalworkflow.Database;
using approvalworkflow.Enums;
using approvalworkflow.Models;
using Microsoft.EntityFrameworkCore;

namespace approvalworkflow.Services;

public class RequestService : IRepositoryService<UserRequest, RequestApproval>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<RequestService> _logger;
    private readonly AppUserService _appUserService;
    private readonly ILookupService<RequestCategory> _requestCategoryService;
    private RequestCategory UNKNOWN_CATEGORY = new();

    public RequestService(AppDbContext dbContext, 
                ILogger<RequestService> logger, 
                AppUserService appUserService, 
                ILookupService<RequestCategory> requestCategoryService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _appUserService = appUserService;
        _requestCategoryService = requestCategoryService;
    }

    public async Task<List<UserRequest>> GetRecordsByUserAsync(ClaimsPrincipal user, Paginated<UserRequest>? paginator = null)
    {
        var currentUser = await _appUserService.AppUserAsync(user);
        var requestsByUser = _dbContext.UserRequests
                .Where(u => u.CreatedById == currentUser.Id);

        if(paginator != null){
            requestsByUser = paginator.GetRecords(requestsByUser);
        }
        return await requestsByUser
                .Include(u => u.Type)
                .Include(u => u.Approvals)
                .ToListAsync();
    }

    public async Task<List<RequestApproval>> GetRecordsForUserAsync(ClaimsPrincipal user)
    {
        
        var currentUser = await _appUserService.AppUserAsync(user);
        if(!currentUser.Roles.Contains(AppRoles.Approver.ToString()))
        {
            return new List<RequestApproval>();
        }
        

        return await _dbContext.RequestApprovals
                    .Where(a => a.ApproverId == currentUser.Id)
                    .Include(a => a.Request)
                        .ThenInclude(r => r.Type)
                    .Include(a => a.Request)
                        .ThenInclude(r => r.CreatedBy)
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

    public async Task<OpResult> CreateRecordAsync(UserRequest newRequest)
    {   
        Func<UserRequest, RequestCategory, Task> createRequestApprovals =  async Task (UserRequest request, RequestCategory requestType) => {
            //get the approvers (supervisor tree)
            var approverList = await _appUserService.GetSupervisorTreeAsync(request.CreatedById, requestType.RequiredApproverCount);
            if(approverList != null)
            {
                request.Approvals = approverList.Select(id => 
                    new RequestApproval{ApproverId = id, Status = ApprovalStatus.Pending}
                ).ToList();
            }

            //set status depending on approver
            request.Status = request.Approvals == null ? 
                                    RequestStatus.Approved : RequestStatus.Pending;
        };

        return await UpsertUserRequestAsync(newRequest, createRequestApprovals);
    }

    public async Task<OpResult> UpdateRecordAsync(UserRequest request)
    {
        return await UpsertUserRequestAsync(request);
        
    }

    public Task<OpResult> UpdateRecordAsync(RequestApproval record)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteRecordAsync(int recordId)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try{
            _dbContext.UserRequests.Remove(new UserRequest{Id = recordId});
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return true;
        }catch(Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return false;
        }   
    }

    private async Task<OpResult> UpsertUserRequestAsync(UserRequest request, Func<UserRequest, RequestCategory, Task>? configureRequestByRequestCategory = null)
    {
        //validate user
        var currentUser = await _appUserService.AppUserAsync(request.User);
        if(currentUser == _appUserService.UNKNOWN_USER)
        {
            //TODO: Add logging
            _logger.LogError(ErrorEventId.UserNotExists, "Currently logged in user is not a registered user.");
            return new OpResult(Success: false, ErrorEventId: ErrorEventId.UserNotExists);
        }

        //validate request type
        var requestType = _requestCategoryService.GetRecord(request.TypeId);
        if(requestType == UNKNOWN_CATEGORY)
        {
            //TODO: add logging             
            return new OpResult(Success: false, ErrorEventId: ErrorEventId.CategoryNotExists);
        }

        var draftRequest = _dbContext.UserRequests.Find(request.Id);
        //validate that the current user owns the request
        if(draftRequest != null && draftRequest.CreatedById != currentUser.Id)
        {
            return new OpResult(Success: false, ErrorEventId: ErrorEventId.UnauthorizedRequestAccess);
        }

        if(draftRequest == null)
        {
            var now = DateTime.UtcNow;
            request.CreatedDate = now;
            request.UpdatedDate = now;
            request.CreatedById = currentUser.Id;
            
            _dbContext.UserRequests.Add(request);
            draftRequest = request;
        }
        else
        {
            draftRequest.Title = request.Title;
            draftRequest.Description = request.Description;
            draftRequest.TypeId = request.TypeId;
            draftRequest.UpdatedDate = DateTime.UtcNow;
        }

        if(configureRequestByRequestCategory != null)
        {
            await configureRequestByRequestCategory(draftRequest, requestType);
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return new OpResult(Success: true, Data: new {requestId = request.Id});
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new OpResult(Success: false, ErrorEventId: ErrorEventId.UnexpectedErrorOnSave);
        }
    }
}

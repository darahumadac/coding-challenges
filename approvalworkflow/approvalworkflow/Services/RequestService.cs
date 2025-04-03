using System.Security.Claims;
using approvalworkflow.Database;
using approvalworkflow.Enums;
using approvalworkflow.Models;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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

    public async Task<List<UserRequest>> GetRecordsByUserAsync(ClaimsPrincipal user)
    {
        var currentUser = await _appUserService.AppUserAsync(user);
        return await _dbContext.UserRequests
                .Where(u => u.CreatedById == currentUser.Id)
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
            var now = DateTime.UtcNow;
            newRequest.CreatedDate = now;
            newRequest.UpdatedDate = now;

            var createdBy = await _appUserService.AppUserAsync(newRequest.User);
            if(createdBy == _appUserService.UNKNOWN_USER)
            {
                //TODO: Add logging
                return new OpResult(Success: false, errorCode: ErrorCode.UserNotExists);
            }

            newRequest.CreatedById = createdBy.Id;

            var requestType = _requestCategoryService.GetRecord(newRequest.TypeId);
            if(requestType == UNKNOWN_CATEGORY)
            {
                //TODO: add logging             
                return new OpResult(Success: false, errorCode: ErrorCode.CategoryNotExists);
            }
            
            //get the approvers (supervisor tree)
            var approverList = await _appUserService.GetSupervisorTreeAsync(newRequest.CreatedById, requestType.RequiredApproverCount);
            if(approverList != null)
            {
                newRequest.Approvals = approverList.Select(id => 
                    new RequestApproval{ApproverId = id, Status = ApprovalStatus.Pending}
                ).ToList();
            }

            //set status depending on approver
            newRequest.Status = newRequest.Approvals == null ? 
                                    RequestStatus.Approved : RequestStatus.Pending;

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                _dbContext.UserRequests.Add(newRequest);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return new OpResult(Success: true);
            } catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await _dbContext.Database.RollbackTransactionAsync();

                return new OpResult(Success: false, ErrorCode.UnexpectedErrorOnSave);
            }
    }

    public async Task<OpResult> UpdateRecordAsync(UserRequest request)
    {
        //TODO: Update implementation. this is a stub
        return new OpResult(Success: true);
    }

    public Task<OpResult> UpdateRecordAsync(RequestApproval record)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteRecordAsync(int recordId)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try{
            _dbContext.UserRequests.Remove(new UserRequest{Id = recordId});
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return true;
        }catch(Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await transaction.RollbackAsync();

            return false;
        }
        
    }
}

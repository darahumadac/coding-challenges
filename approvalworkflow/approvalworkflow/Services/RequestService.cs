using System;
using System.Security.Claims;
using approvalworkflow.Database;
using approvalworkflow.Enums;
using approvalworkflow.Models;
using Humanizer;
using Microsoft.AspNetCore.Identity;

namespace approvalworkflow.Services;

public class RequestService : IRepositoryService<UserRequest>
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _dbContext;
    private readonly User UNKNOWN_USER = new();
    
    public RequestService(UserManager<User> userManager, AppDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;

    }

    public async Task<List<UserRequest>> GetRecordsByUserAsync(ClaimsPrincipal currentUser)
    {
        var user = await _userManager.GetUserAsync(currentUser) ?? UNKNOWN_USER;
        var approvers = await _userManager.GetUsersInRoleAsync(AppRoles.Approver.ToString());
        var random = new Random();
        var stubRequests = Enumerable.Range(1,100)
            .Select(i => {
                var reqType = (RequestType) random.Next((int)RequestType.Service, (int)RequestType.RoleChange + 1);
                return new UserRequest{
                    Id = i, 
                    Title = $"Service Request - Need help #{i} with {reqType.Humanize().Titleize()} request", 
                    Type = new RequestCategory(){RequestType = reqType, RequiredApproverCount = 2},
                    Status = RequestStatus.Pending,
                    CreatedBy = new AppUser{FirstName = user.FirstName, LastName = user.LastName},
                    CreatedDate = DateTime.Now.AddDays(random.Next(-5, -3)),
                    UpdatedDate = DateTime.Now.AddDays(random.Next(-2, 0)),
                    Approvals = Enumerable.Range(1, 2).Select(a => new RequestApproval{Approver = new AppUser{FirstName = "Approver 1"}}).ToList()
                };
            }).ToList();    

        return stubRequests;
    }

    public async Task<List<UserRequest>> GetRecordsForUserAsync(ClaimsPrincipal currentUser)
    {
        var requestors = await _userManager.GetUsersInRoleAsync(AppRoles.Requestor.ToString());
        var requestor = requestors.First() ?? UNKNOWN_USER;
        var random = new Random();
        var reqType = currentUser.IsInRole(AppRoles.Admin.ToString()) ? RequestType.RoleChange : RequestType.Service;
        var forApproval = Enumerable.Range(1,5)
                    .Select(i => {
                        return new UserRequest{
                            Id = i,
                             Title = $"Service Request - Need help #{i} with {reqType.Humanize().Titleize()} request", 
                            Type = new RequestCategory(){RequestType = reqType, RequiredApproverCount = 2}, 
                            Status = RequestStatus.Pending,
                            CreatedBy = new AppUser{FirstName = requestor.FirstName, LastName = requestor.LastName},
                            CreatedDate = DateTime.Now.AddDays(random.Next(-5, -3)),
                            UpdatedDate = DateTime.Now.AddDays(random.Next(-2, 0)),
                        };
                    }).ToList();
        return forApproval;
    }
}

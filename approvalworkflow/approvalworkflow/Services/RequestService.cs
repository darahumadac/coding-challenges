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
    private readonly User UNKNOWN_USER = new();
    
    public RequestService(UserManager<User> userManager)
    {
        //TODO: add app db here
        _userManager = userManager;

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
                    Type = reqType,
                    Status = RequestStatus.Pending,
                    CreatedBy = user,
                    CreatedDate = DateTime.Now.AddDays(random.Next(-5, -3)),
                    UpdatedDate = DateTime.Now.AddDays(random.Next(-2, 0)),
                    Approvers = (List<User>)approvers
                };
            }).ToList();    

        return stubRequests;
    }

    public async Task<List<UserRequest>> GetRecordsForUserAsync(ClaimsPrincipal currentUser)
    {
        var requestors = await _userManager.GetUsersInRoleAsync(AppRoles.Requestor.ToString());
        var random = new Random();
        var reqType = currentUser.IsInRole(AppRoles.Admin.ToString()) ? RequestType.RoleChange : RequestType.Service;
        var forApproval = Enumerable.Range(1,5)
                    .Select(i => {
                        return new UserRequest{
                            Id = i,
                             Title = $"Service Request - Need help #{i} with {reqType.Humanize().Titleize()} request", 
                            Type = reqType, 
                            Status = RequestStatus.Pending,
                            CreatedBy = requestors != null ? requestors.First() : UNKNOWN_USER,
                            CreatedDate = DateTime.Now.AddDays(random.Next(-5, -3)),
                            UpdatedDate = DateTime.Now.AddDays(random.Next(-2, 0)),
                        };
                    }).ToList();
        return forApproval;
    }
}

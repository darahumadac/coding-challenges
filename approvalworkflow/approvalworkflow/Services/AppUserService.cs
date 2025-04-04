using System.Security.Claims;
using approvalworkflow.Database;
using approvalworkflow.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace approvalworkflow.Services;

public class AppUserService
{
    private readonly AppDbContext _appDbContext;
    private readonly UserManager<User> _userManager;
    public readonly AppUser UNKNOWN_USER = new();

    public AppUserService(AppDbContext appDbContext, UserManager<User> userManager)
    {
        _appDbContext = appDbContext;
        _userManager = userManager;
    }
    public async Task<AppUser> AppUserAsync(ClaimsPrincipal currentUser)
    {
        var user = await _userManager.GetUserAsync(currentUser);
        if (user == null)
        {
            return UNKNOWN_USER;
        }

        var appUser = await _appDbContext.AppUsers.Where(u => u.AuthUserId == user.Id)
                                                // .Include(u => u.Supervisor)
                                                .AsNoTracking()
                                                .FirstAsync() ?? UNKNOWN_USER;
        var userRoles = await _userManager.GetRolesAsync(user);
        if (appUser != UNKNOWN_USER)
        {
            appUser.Roles = [.. userRoles];
        }
        return appUser;

    }

    public async Task<List<int>?> GetSupervisorTreeAsync(int appUserId, int? depth = null)
    {
        var appUser = await _appDbContext.AppUsers.Where(u => u.Id == appUserId).Include(u => u.Supervisor).FirstOrDefaultAsync() ?? UNKNOWN_USER;
        var approver = appUser.Supervisor;
        List<int>? approverIds = null;
        if (approver != null)
        {
            approverIds = new List<int>();
            while (approver != null && (depth == null || approverIds.Count < depth) )
            {
                approverIds.Add(approver.Id);
                appUser = await _appDbContext.AppUsers.Where(u => u.Id == approver.Id).Include(u => u.Supervisor).FirstAsync();
                approver = appUser.Supervisor;
            }
        }
        return approverIds;
    }
}
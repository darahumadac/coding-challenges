using System;
using approvalworkflow.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace approvalworkflow.Database;

public class AppDbContext : DbContext
{
    private readonly UserManager<User> _userManager;
    public AppDbContext(DbContextOptions<AppDbContext> options, UserManager<User> userManager) : base(options){
        _userManager = userManager;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSeeding((dbContext, _) => {
            //seed users from users db
            var appUsers = dbContext.Set<AppUser>();
            //seed app approvers
            var approvers = _userManager.GetUsersInRoleAsync(AppRoles.Approver.ToString()).GetAwaiter().GetResult();
            appUsers.AddRange(approvers.Select(a => new AppUser{FirstName = a.FirstName, LastName = a.LastName}));
            dbContext.SaveChanges();

            //seed app requestor
            var requestors = _userManager.GetUsersInRoleAsync(AppRoles.Requestor.ToString()).GetAwaiter().GetResult();
            var random = new Random();
            int max = appUsers.Count() + 1;
            appUsers.AddRange(requestors.Select(r => {
                var supervisorId = random.Next(1, max);
                return new AppUser{
                FirstName = r.FirstName, 
                LastName = r.LastName, 
                Supervisor = appUsers.First(a => a.Id == supervisorId)
            };
            }));
            dbContext.SaveChanges();
        });
    }

    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<UserRequest> UserRequests { get; set; }
    public DbSet<RequestCategory> RequestCategories { get; set; }
    public DbSet<RequestApproval> RequestApprovals { get; set; }

    

}

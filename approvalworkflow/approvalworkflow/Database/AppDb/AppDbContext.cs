using System;
using approvalworkflow.Enums;
using approvalworkflow.Models;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace approvalworkflow.Database;

public class AppDbContext : DbContext
{
    private readonly UserManager<User> _userManager;
    public AppDbContext(DbContextOptions<AppDbContext> options, UserManager<User> userManager) : base(options)
    {
        _userManager = userManager;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<RequestCategory>()
                    .Property(r => r.RequestType)
                    .HasConversion<string>();             
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSeeding((dbContext, _) =>
        {
            //seed the request categories
            var categories = dbContext.Set<RequestCategory>();
            if (categories.Count() == 0)
            {
                categories.AddRange(Enum.GetValues(typeof(RequestType))
                                .Cast<RequestType>()
                                .Select((rt, i) => new RequestCategory { RequestType = rt, RequiredApproverCount = i + 1 }));
                dbContext.SaveChanges();
            }


            //seed users from users db
            var appUsers = dbContext.Set<AppUser>();
            if (appUsers.Count() > 0)
            {
                return;
            }
            //seed app approvers
            var approvers = _userManager.GetUsersInRoleAsync(AppRoles.Approver.ToString()).GetAwaiter().GetResult();
            appUsers.AddRange(approvers.Select(a => new AppUser { AuthUserId = a.Id, FirstName = a.FirstName, LastName = a.LastName }));
            dbContext.SaveChanges();

            //seed app requestor
            var requestors = _userManager.GetUsersInRoleAsync(AppRoles.Requestor.ToString()).GetAwaiter().GetResult();
            var random = new Random();
            int max = appUsers.Count() + 1;
            appUsers.AddRange(requestors.Select(r =>
            {
                var supervisorId = random.Next(1, max);
                return new AppUser
                {
                    AuthUserId = r.Id,
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

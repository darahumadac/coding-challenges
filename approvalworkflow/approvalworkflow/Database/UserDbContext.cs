using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace approvalworkflow.Database;

public class UserDbContext : IdentityDbContext<User>
{
    private readonly IConfiguration _config;
    public UserDbContext(DbContextOptions options, IConfiguration config) : base(options)
    {
        _config = config;
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        //rename identity tables
        builder.Entity<User>().ToTable(name: "Users");
        builder.Entity<IdentityRole>().ToTable(name: "Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable(name: "UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable(name: "UserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable(name: "UserLogins");
        builder.Entity<IdentityRoleClaim<string>>().ToTable(name: "RoleClaims");
        builder.Entity<IdentityUserToken<string>>().ToTable(name: "UserTokens");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSeeding((dbContext, _) =>
        {

            var pwHasher = new PasswordHasher<User>();

            //seed users
            var users = dbContext.Set<User>();
            if (users.IsNullOrEmpty())
            {
                var defaultPassword = _config["UsersDb:Users:DefaultPassword"] ??
                                throw new InvalidOperationException("No default user password set");
                //seed requestors and approvers
                for (var i = 0; i < 5; i++)
                {
                    string role = i < 2 ? "Requestor" : "Approver";
                    var user = new User
                    {
                        UserName = $"Test_{role}_{i + 1}",
                        FirstName = "Test",
                        LastName = $"{role} {i + 1}",
                        Email = $"test{role.ToLower()}_{i + 1}@test.test",
                        EmailConfirmed = true
                    };
                    user.PasswordHash = pwHasher.HashPassword(user, defaultPassword);
                    users.Add(user);
                }

                //seed admin
                var adminPassword = _config["UsersDb:Admin:DefaultPassword"] ??
                                    throw new InvalidOperationException("No default admin password set");
                var admin = new User
                {
                    UserName = "Admin",
                    FirstName = "Admin",
                    LastName = "Admin",
                    Email = "admin@test.test",
                    EmailConfirmed = true,
                };
                admin.PasswordHash = pwHasher.HashPassword(admin, adminPassword);
                users.Add(admin);
            }

            //seed roles
            var roles = dbContext.Set<IdentityRole>();
            if (roles.IsNullOrEmpty())
            {
                var appRoles = Enum.GetNames(typeof(AppRoles))
                                .Select(r => new IdentityRole(r)).ToArray();
                roles.AddRange(appRoles);
            }

            dbContext.SaveChanges();

            //seed user roles
            var userRoles = dbContext.Set<IdentityUserRole<string>>();
            if (userRoles.IsNullOrEmpty())
            {
                var appRoles = Enum.GetNames(typeof(AppRoles));
                foreach (var appRole in appRoles)
                {
                    var appRoleId = roles.FirstOrDefault(r => r.Name == appRole.ToString())?.Id;
                    if (appRoleId != null)
                    {
                        var appRoleUsers = users.Where(u => u.UserName != null && u.UserName.Contains(appRole.ToString()))
                                                        .Select(u => new IdentityUserRole<string> { RoleId = appRoleId, UserId = u.Id });
                        userRoles.AddRange(appRoleUsers);
                    }
                }
                dbContext.SaveChanges();
            }

        });
    }
}
using System.Runtime;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace approvalworkflow.Database;

public class UserDbContext : IdentityDbContext<User>
{
    private readonly IConfiguration _config;
    public UserDbContext(DbContextOptions<UserDbContext> options, IConfiguration config) : base(options)
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
        optionsBuilder.UseSeeding((dbContext, _) =>
        {
            var normalize = (string input) => input.ToUpper();
            var generateEmail = (string input) => $"{input}@test.test";
            var generateUsername = (params string[] args) => string.Join("_", args).ToLower().TrimEnd('_');
            var pwHasher = new PasswordHasher<User>();

            var createUser = (int i) =>
            {

                const int APPROVER = 2;
                const int ADMIN = 5;
                const string ADMIN_ROLE = "Admin";

                var role = "Approver";
                var firstName = "Test";
                if (i < APPROVER)
                {
                    role = "Requestor";
                }
                var lastName = $"{role}_{i + 1}";

                if (i == ADMIN)
                {
                    role = ADMIN_ROLE;
                    firstName = ADMIN_ROLE;
                    lastName = string.Empty;
                }

                var username = generateUsername(firstName, lastName);
                var email = generateEmail(username);

                var key = i != ADMIN ? "Users" : ADMIN_ROLE;
                var defaultPassword = _config[$"UsersDb:{key}:DefaultPassword"] ??
                                throw new InvalidOperationException($"No default {key.ToLower()} password set");

                var user = new User
                {
                    UserName = username,
                    NormalizedUserName = normalize(username),
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    NormalizedEmail = normalize(email),
                    EmailConfirmed = true
                };
                user.PasswordHash = pwHasher.HashPassword(user, defaultPassword);

                return user;
            };

            //seed users
            var users = dbContext.Set<User>();
            if (users.IsNullOrEmpty())
            {
                //seed requestors and approvers
                for (var i = 0; i < 6; i++)
                {
                    users.Add(createUser(i));
                }
            }

            //seed roles
            var roles = dbContext.Set<IdentityRole>();
            if (roles.IsNullOrEmpty())
            {
                var appRoles = Enum.GetNames(typeof(AppRoles))
                                .Select(r => new IdentityRole { Name = r, NormalizedName = r.ToUpper() }).ToArray();
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
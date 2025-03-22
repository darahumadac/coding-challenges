# Notes

## Adding Authentication and Authorization
- To create an MVC app with authentication, add the `--auth` options
```bash
dotnet new mvc --auth Individual #individual authentication
dotnet new mvc --auth Windows #windows authentication
```
- To add authorization and authentication manually:
1. Add the following packages:
```bash
# packages for adding database
dotnet add package Microsoft.EntityFrameworkCore.SqlServer #your preferred db
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools
#package for adding entity framework core identity framework
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore # important to add this so that the entity framework store can be added
#package for adding identity ui
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
dotnet add package Microsoft.AspNetCore.Identity.UI
```

2. For Individual Authentication with Roles, create the `IdentityUser` and `IdentityDbContext`:
   1. Can extend the `IdentityUser` class to add other properties for the application user
   2. Configure database that will be used as the `IdentityDbContext<TUser>`
   3. Create DbContext that extends the `IdentityDbContext<TUser>` where `TUser` is an `IdentityUser`
```c#
//extended the IdentityUser to add FirstName and LastName
public class User : IdentityUser
{
    [Required]
    public string FirstName { get; set; } = null!;
    [Required]
    public string LastName { get; set; } = null!;
}

//extend IdentityDbContext<TUser>
public class UserDbContext : IdentityDbContext<User>
{
    public UserDbContext(DbContextOptions options) : base(options) { }
    //Optional - Override OnModelCreating to rename tables
    //Optional - Override OnConfiguring to seed data
}
```
3. (Optional) Change the name of the `IdentityDbContext` tables by overriding the `OnModelCreating(ModelBuilder builder)` method
```c#
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
```
4. (Optional) Seed the identity db context with data by overriding the `OnConfiguring(DbContextOptionsBuilder optionsBuilder)` of the `IdentityDbContext` and using `optionsBuilder.UseSeeding((context, _) => {/*seed data here*/})`
```c#
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    base.OnConfiguring(optionsBuilder);
    optionsBuilder.UseSeeding((dbContext, _) =>
    {
        //seed roles
        var roles = dbContext.Set<IdentityRole>();
        if (roles.IsNullOrEmpty())
        {
            var appRoles = Enum.GetNames(typeof(AppRoles))
                            .Select(r => new IdentityRole(r)).ToArray();
            roles.AddRange(appRoles);
        }
        dbContext.SaveChanges();
    });
}
```
5. Configure the `IdentityDbContext`, and identity services
```c#
//configure the identity database
builder.Services.AddDbContext<UserDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("UserDB")));
//configure identity services
builder.Services.AddIdentity<User, IdentityRole>() //add identity with role
                .AddEntityFrameworkStores<UserDbContext>() //set the identity db
                .AddDefaultUI() //add the default ui
                .AddApiEndpoints(); //configure identity apis
```
6. Update the database
```bash
dotnet ef migrations add InitDb
dotnet ef database update
```
7. Scaffold the Identity Views
```bash
# install the dotnet-aspnet-codegenerator
dotnet tool install -g dotnet-aspnet-codegenerator

# run the code generator. this will generate an Area folder for Identity pages
dotnet aspnet-codegenerator identity --dbContext {yourDbContext}
```
7. Wire up the scaffolded views in `Program.cs`
```c#
builder.Services.AddRazorPages();
//after app.Build() and after mapping controller route
app.MapRazorPages();
app.Run();
```

### Update Register / Login if IdentityUser was extended
- Need to update the view and logics in Register / Login pages accordingly


### Customizing Identity
https://learn.microsoft.com/en-us/aspnet/core/security/authentication/customize-identity-model?view=aspnetcore-9.0
- What is the difference between `AddIdentity<TUser, TRole>`, `AddDefaultIdentity<TUser>`, `AddIdentityCore<TUser>`?

### Seeding Data in EF
- Ideal approach is to override `OnConfiguring(DbContextOptionsBuilder optionsBuilder)` and use the `optionsBuilder.UseSeeding((context, _) => {/*seeding logic here*/})`
- The `OnConfiguring` method always runs during a migration


### External Login Providers
#### Google
Steps: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins?view=aspnetcore-9.0
- In the google console, create a new project for your app and go through the steps
- Add the **Authorized Redirect URI** for your app: http://localhost:5027/signin-oidc
- Take note of the ClientId and ClientSecret and store them in the user-secrets when working in dev environment
```bash
# initialize user-secrets
dotnet user-secrets init
# set the secrets
dotnet user-secrets set "Authentication:Google:ClientId" "<client-id>"
dotnet user-secrets set "Authentication:Google:ClientSecret" "<client-secret>"
```
- Configure Google in `AddAuthentication`
```c#
// configure external login providers
builder.Services.AddAuthentication()
                .AddGoogleOpenIdConnect(options =>
                {
                    //set the nonce cookie
                    options.NonceCookie = new CookieBuilder
                    {
                        Name = "GoogleAuthCookie", 
                        SameSite = SameSiteMode.None, 
                        SecurePolicy = CookieSecurePolicy.Always
                    };
                    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
                    // options.ProtocolValidator.RequireNonce = false;//this works but don't use this
                });

//then use Authentication
app.UseAuthentication();
```

### Routing
- Adding the `[Route(path/to/{routeValue})]` attribute or just specifying the route template in HTTP attributes will make it appear in swagger / swashbuckle
- The attributes below are mutually exclusive:
    - `asp-route`
    - `asp-controller`, `asp-action`
    - `asp-page`, `asp-page-handler`

- Action method names must always match the View filename
- Views are searched in the  following order: `Views/[Controller] directory` -> `Views/Shared directory` -> `Pages/Shared directory`. The view filename searched is the action name
- `asp-route-id` - binds route value

### Model Binding
https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-9.0#binding-source-parameter-inference
- `[FromQuery]` - Gets values from the query string.
- `[FromRoute]` - Gets values from route data.
- `[FromForm]` - Gets values from posted form fields.
- `[FromBody]` - Gets values from the request body.
- `[FromHeader]` - Gets values from HTTP headers.

#### Tag Helpers
https://learn.microsoft.com/en-us/aspnet/core/mvc/views/working-with-forms?view=aspnetcore-9.0
```html

<a asp-controller="Request" asp-action="Edit" asp-route-id="@request.Id" class="btn btn-outline-warning btn-sm">Edit</a>

<a asp-route="ViewRequest" asp-route-requestId="@request.Id" class="btn btn-outline-primary btn-sm">View</a>

<a asp-route-ViewRequest asp-route-requestId="@request.Id" class="btn btn-outline-primary btn-sm">View</a>

```
```cshtml
@Url.Action("action_name", "controller_name")
@Url.Action("action_name", "controller_name", route_value)
@Url.RouteUrl("route_name")
```



#### Showing modal using partial view
```c#
    [HttpGet("ConfirmDelete/{requestId}")]
    public IActionResult _ConfirmDelete(int requestId)
    {
        return PartialView("_ConfirmDeletePartial", requestId);
    }
```
```html
<div id="modal-container">
    <!-- to be populated by the ajax call -->
</div>
```
```js
 //call ajax for loading partial view
$.ajax({
    method:"GET",
    url:`ConfirmDelete/${e.target.id}`,
    success: function(data){
        $('#modal-container').html(data);
        $('#confirmDeleteModal').modal('toggle');
    },
    error: function(){
        alert('error loading modal');
    }
});
```


### Code snippets
- For setting PasswordHash using `PasswordHasher<TUser>`
```c#
// set temp password
var pwh = new PasswordHasher<User>();
var pwUsers = users.Where(u => u.PasswordHash == null)
    .Select(u => new { User = u, NewPwHash = pwh.HashPassword(u, "Test123!") })
    .ToList();

foreach (var pwu in pwUsers)
{
    users.Where(u => u.Id == pwu.User.Id)
        .ExecuteUpdate(setters => setters
        .SetProperty(u => u.PasswordHash, pwu.NewPwHash));
}
```
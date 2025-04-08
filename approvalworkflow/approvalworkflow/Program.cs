using approvalworkflow.Database;
using approvalworkflow.Models;
using approvalworkflow.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

//configure app db
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("AppDB")));

//configure identity services for authentication and authorization
builder.Services.AddDbContext<UserDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("UserDB"))); //add the user db
builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<UserDbContext>()
                .AddDefaultUI()
                .AddApiEndpoints(); //configure identity apis

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
                    // options.ProtocolValidator.RequireNonce = false;//this works but not advisable
                });

//configure email sender
builder.Services.AddTransient<IEmailSender, MockEmailSender>();

builder.Services.AddScoped<IRepositoryService<UserRequest,RequestApproval>, RequestService>();
builder.Services.AddScoped<AppUserService>();
builder.Services.AddScoped<UIService>();
builder.Services.AddScoped<ILookupService<RequestCategory>, RequestCategoryService>();


if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddOpenApi();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    //add swagger for dev
    //this will only have contents if the app.MapIdentityApi<User> is exposed
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
//make the identity api endpoints available
app.MapIdentityApi<User>();


app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();

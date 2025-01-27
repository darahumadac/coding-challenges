using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using urlshortener.service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//configure database
var dbConnectionString = builder.Configuration.GetConnectionString("UrlShortenerDB") ??
                        throw new InvalidOperationException("No 'UrlShortenerDB' connection string found");
builder.Services.AddDbContext<UrlShortenerDbContext>(options => options.UseSqlServer(dbConnectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Endpoints
app.MapPost("/shorten", (ShortenRequest request, HttpContext context, UrlShortenerDbContext db, LinkGenerator url) =>
{
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(request);
    if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
    {
        return Results.BadRequest(validationResults);
    }

    (ResultCode result, string encodedUrl) = new UrlShortener(db).ShortenUrl(request.LongUrl);
    if (result == ResultCode.ERROR)
    {
        return Results.StatusCode(500);
    }

    string createdAtLocation = url.GetUriByName(context, "GetUrl", new { shortUrl = encodedUrl }) ??
                        $"{context.Request.Scheme}://{context.Request.Host}/{encodedUrl}";
    var response = new ShortenResponse(request.LongUrl, createdAtLocation);
    if (result == ResultCode.EXISTS)
    {
        return Results.Ok(response);
    }
    return Results.CreatedAtRoute(routeName: "GetUrl", routeValues: new { shortUrl = encodedUrl }, response);
});


app.MapGet("/{shortUrl}", (string shortUrl, UrlShortenerDbContext db) =>
{
    (ResultCode result, string longUrl) = new UrlShortener(db).GetLongUrl(shortUrl);
    if (result == ResultCode.NOT_FOUND)
    {
        return Results.NotFound();
    }
    // return Results.Redirect(longUrl, true);
    return Results.Json(longUrl); //TODO: Remove this, and replace with Permanent Redirect
}).WithName("GetUrl");

app.Run();


public record ShortenRequest
{
    [Url]
    [Required]
    public string LongUrl { get; init; } = string.Empty;
}
public record ShortenResponse(string longUrl, string shortUrl);

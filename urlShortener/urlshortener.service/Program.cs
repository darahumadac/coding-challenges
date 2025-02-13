using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using middleware;
using Serilog;
using Serilog.Context;
using StackExchange.Redis;
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

//configure redis for caching
var cacheConnectionString = builder.Configuration.GetConnectionString("UrlShortenerCache") ?? "localhost";
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(cacheConnectionString));

//configure UrlShortenerService so dependencies can be injected, and this can be injected as dependency
builder.Services.AddScoped<UrlShortener>();

//configure cors - Cross-Origin Resource Sharing
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
            .WithMethods("POST", "GET")
            .WithHeaders("Content-Type", "Accept");
    });
});

//configure logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.RequestBody | HttpLoggingFields.ResponseBody;
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});
builder.Services.AddSerilog(); //injects serilog to Microsoft.Extensions.Logging.ILogger<T>

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
//use serilog
app.UseMiddleware<RequestContextMiddleware>();
app.UseHttpLogging();
app.UseSerilogRequestLogging();
//use cors
app.UseCors();

app.MapGet("/", () => Results.Ok("API is up"));

// Endpoints
app.MapPost("/shorten", (ShortenRequest request, HttpContext context, UrlShortenerDbContext db, LinkGenerator url, UrlShortener urlShortener) =>
{
    //adding this will cause all log events within this block to have EndpointEventName property
    using (LogContext.PushProperty("EndpointEventName", "RequestValidation"))
    {
        app.Logger.LogInformation("Logging from endpoint");
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(request);
        if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
        {
            app.Logger.LogError("Failed validation");
            return Results.BadRequest(validationResults);
        }

        app.Logger.LogInformation("Passed validation");
    }

    //this will not have "EndpointEventName" property anymore because it is outside the block
    app.Logger.LogInformation("Trying to shorten url");

    (ResultCode result, string encodedUrl) = urlShortener.ShortenUrl(request.LongUrl);
    if (result == ResultCode.ERROR)
    {
        app.Logger.LogError("Something went wrong");
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


app.MapGet("/{shortUrl}", (string shortUrl, UrlShortenerDbContext db, UrlShortener urlShortener) =>
{
    (ResultCode result, string longUrl) = urlShortener.GetLongUrl(shortUrl);
    if (result == ResultCode.NOT_FOUND)
    {
        return Results.NotFound();
    }
    app.Logger.LogInformation($"Redirecting to {longUrl}");
    return Results.Redirect(longUrl, permanent: true);
}).WithName("GetUrl");

app.Run();


public record ShortenRequest
{
    [Url]
    [Required]
    public string LongUrl { get; init; } = string.Empty;
}
public record ShortenResponse(string longUrl, string shortUrl);

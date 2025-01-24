using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using urlshortener.service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Endpoints
app.MapPost("/shorten", (ShortenRequest request) =>
{
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(request);
    if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
    {
        return Results.BadRequest(validationResults);
    }

    string shortUrl = UrlShortener.ShortenUrl(request.LongUrl);
    return Results.Created(shortUrl, new ShortenResponse(request.LongUrl, shortUrl));
});

app.MapGet("/{shortUrl}", (string shortUrl) =>
{
    //TODO: 301 redirect
    return shortUrl;
});

app.Run();


public record ShortenRequest
{
    [Url]
    [Required]
    public string LongUrl { get; init; } = string.Empty;
}
public record ShortenResponse(string longUrl, string shortUrl);

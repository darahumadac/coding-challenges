using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace urlshortener.service;

public class UrlShortenerDbContext : DbContext
{
    public UrlShortenerDbContext(DbContextOptions options) : base(options) { }
    public UrlShortenerDbContext() { }

    public virtual DbSet<UrlMapping> UrlMappings { get; set; }
}

[PrimaryKey(nameof(LongUrl))]
[Index(nameof(ShortUrl))]
public class UrlMapping
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; init; }
    public string LongUrl { get; init; } = string.Empty;
    public string ShortUrl { get; set; } = string.Empty;

}
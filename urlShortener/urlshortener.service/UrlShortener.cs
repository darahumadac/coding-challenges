using UrlShortenerResult = (ResultCode code, string shortUrl);
using System.Text.RegularExpressions;
using Serilog;

namespace urlshortener.service;

public class UrlShortener
{
    private readonly Func<long> _generateId;
    private readonly Regex _allowedChars;
    private readonly UrlShortenerDbContext _db;
    private readonly static Regex _lettersAndDigits = new Regex(@"[a-zA-Z0-9]");
    private readonly char[] _validChars;
    public Serilog.ILogger _logger;

    public UrlShortener(Func<long>? generateId, Regex? allowedChars, UrlShortenerDbContext db)
    {
        _generateId = generateId ?? IdGenerator.GenerateId;
        _allowedChars = allowedChars ?? _lettersAndDigits;
        //get ascii characters (0-127) in the allowed characters regex
        _validChars = Enumerable.Range(0, 128)
                        .Select(i => (char)i)
                        .Where(c => _allowedChars.IsMatch(c.ToString()))
                        .ToArray();
        _db = db;
        _logger = Log.ForContext<UrlShortener>();

    }

    public UrlShortener(UrlShortenerDbContext db) : this(IdGenerator.GenerateId, _lettersAndDigits, db) { }

    public UrlShortenerResult ShortenUrl(string longUrl)
    {

        _logger.Information("testing darah");
        var mapping = _db.UrlMappings.Find(longUrl);
        // var mapping = _db.UrlMappings.FirstOrDefault(u => u.LongUrl == longUrl);
        _logger
            .ForContext("EventName", "LookupLongUrl")
            .Information("Done finding longUrl in mapping");

        if (mapping != null)
        {
            _logger
                .ForContext("EventName", "ExistsLongUrl")
                .Information("LongURL exists in database");

            return (ResultCode.EXISTS, mapping.ShortUrl);
        }

        try
        {
            // generate shortened url
            long id = _generateId();
            //encode to appropriate base
            string shortenedUrl = convertToBase(id, _validChars.Length);
            _db.UrlMappings.Add(new UrlMapping() { LongUrl = longUrl, ShortUrl = shortenedUrl });
            _db.SaveChanges();

            _logger
                .ForContext("EventName", "ShortenUrlSuccess")
                .Information("Saved shortened url in database");

            return (ResultCode.OK, shortenedUrl);
        }
        catch (Exception ex)
        {
            _logger
                .ForContext("EventName", "ShortUrlGenerationException")
                .ForContext("Error", ex)
                .Error("Exception occurred");
            return (ResultCode.ERROR, string.Empty);
        }

    }

    private string convertToBase(long id, int numBase)
    {
        string result = string.Empty;
        long quotient = id;
        while (quotient != 0)
        {
            result = $"{_validChars[quotient % numBase]}{result}";
            quotient /= numBase;
        }
        return result;
    }

    public UrlShortenerResult GetLongUrl(string shortUrl)
    {
        var mapping = _db.UrlMappings.FirstOrDefault(m => m.ShortUrl == shortUrl);
        if (mapping != null)
        {
            _logger
                .ForContext("EventName", "FoundShortUrl")
                .Information("short url exists in database");
            return (ResultCode.OK, mapping.LongUrl);
        }
        _logger
           .ForContext("EventName", "ShortUrlNotFound")
           .Error("short not found in database");
        return (ResultCode.NOT_FOUND, string.Empty);
    }
}
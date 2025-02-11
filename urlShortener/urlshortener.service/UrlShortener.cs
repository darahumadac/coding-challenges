using UrlShortenerResult = (ResultCode code, string shortUrl);
using System.Text.RegularExpressions;
using Serilog;
using StackExchange.Redis;

namespace urlshortener.service;

public class UrlShortener
{
    private readonly Func<long> _generateId;
    private readonly Regex _allowedChars;
    private readonly UrlShortenerDbContext _db;
    private readonly static Regex _lettersAndDigits = new Regex(@"[a-zA-Z0-9]");
    private readonly char[] _validChars;
    private Serilog.ILogger _logger;

    private IDatabase _redis;
    private const string SHORTURL_MAPPING_KEY = "SHORTURL_MAPPING";


    public UrlShortener(Func<long>? generateId, Regex? allowedChars, UrlShortenerDbContext db, IConnectionMultiplexer mux)
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
        _redis = mux.GetDatabase();
    }

    public UrlShortener(UrlShortenerDbContext db, IConnectionMultiplexer mux) : this(IdGenerator.GenerateId, _lettersAndDigits, db, mux) { }

    public UrlShortenerResult ShortenUrl(string longUrl)
    {
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
            //add to cache and with 10 minutes expiry
            _logger.Information("Saving shortenedUrl to cache");
            _redis.HashSet(SHORTURL_MAPPING_KEY, shortenedUrl, longUrl);
            _redis.HashFieldExpire(SHORTURL_MAPPING_KEY, [shortenedUrl], TimeSpan.FromSeconds(600));

            //save to db
            _logger.Information("Saving shortenedUrl to db");
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
        //check cache. if shorturl is not in cache, check db
        var longUrl = (string?)_redis.HashGet(SHORTURL_MAPPING_KEY, shortUrl);
        if (longUrl == null)
        {
            _logger.Debug("shorturl not in cache, checking db");
            var mapping = _db.UrlMappings.FirstOrDefault(m => m.ShortUrl == shortUrl);
            if (mapping != null)
            {
                _logger
                    .ForContext("EventName", "FoundShortUrl")
                    .Information("short url exists in database");

                longUrl = mapping.LongUrl;
                //add to cache
                _logger.Debug("adding shorturl to cache");
                _redis.HashSet(SHORTURL_MAPPING_KEY, shortUrl, longUrl);
                _redis.HashFieldExpire(SHORTURL_MAPPING_KEY, [shortUrl], TimeSpan.FromSeconds(600));
            }
        }
        else
        {
            _logger
                .ForContext("EventName", "FoundShortUrl")
                .Debug("short url exists in cache");
        }

        var code = ResultCode.OK;
        if (longUrl == null)
        {
            _logger
            .ForContext("EventName", "ShortUrlNotFound")
            .Error("short not found in database");

            code = ResultCode.NOT_FOUND;
            longUrl = string.Empty;
        }

        return (code, longUrl);

    }
}
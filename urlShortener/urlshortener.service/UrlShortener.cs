using UrlShortenerResult = (ResultCode code, string shortUrl);
using System.Text.RegularExpressions;

namespace urlshortener.service;

public class UrlShortener
{
    private readonly Func<long> _generateId;
    private readonly Regex _allowedChars;
    private readonly UrlShortenerDbContext _db;
    private readonly static Regex _lettersAndDigits = new Regex(@"[a-zA-Z0-9]");
    private readonly char[] _validChars;

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
    }

    public UrlShortener(UrlShortenerDbContext db) : this(IdGenerator.GenerateId, _lettersAndDigits, db) { }

    public UrlShortenerResult ShortenUrl(string longUrl)
    {
        var mapping = _db.UrlMappings.Find(longUrl);
        // var mapping = _db.UrlMappings.FirstOrDefault(u => u.LongUrl == longUrl);

        if (mapping != null)
        {
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

            return (ResultCode.OK, shortenedUrl);
        }
        catch (Exception ex)
        {
            //TODO: log error
            //LOG ex.Message
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
            return (ResultCode.OK, mapping.LongUrl);
        }
        return (ResultCode.NOT_FOUND, string.Empty);
    }
}
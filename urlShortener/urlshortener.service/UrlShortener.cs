using System.IO.Hashing;
using System.Text;
using System.Text.RegularExpressions;

namespace urlshortener.service;

public class UrlShortener
{
    private readonly Func<long> _generateSalt;
    private readonly Regex _allowedChars;
    private readonly static Func<long> _getUnixTsMs = () => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    private readonly static Regex _lettersAndDigits = new Regex(@"[a-zA-Z0-9]");
    private readonly char[] _validChars;

    public UrlShortener(Func<long>? generateSalt, Regex? allowedChars)
    {
        _generateSalt = generateSalt ?? _getUnixTsMs;
        _allowedChars = allowedChars ?? _lettersAndDigits;
        //get ascii characters (0-127) in the allowed characters regex
        _validChars = Enumerable.Range(0, 128)
                        .Select(i => (char)i)
                        .Where(c => _allowedChars.IsMatch(c.ToString()))
                        .ToArray();
    }

    public UrlShortener() : this(_getUnixTsMs, _lettersAndDigits) { }

    public string ShortenUrl(string longUrl)
    {
        //TODO: check if longurl is not in db. if it is, return the short url. if not, generate new id and shorten url
        //TODO: only generate id if longurl is not in db

        long salt = _generateSalt();
        //hash url to crc32 which gives a hex, and convert it to long
        long encodedUrl = Int64.Parse(
            Convert.ToHexString(Crc32.Hash(Encoding.ASCII.GetBytes(longUrl))),
            System.Globalization.NumberStyles.HexNumber);

        //encode to appropriate base
        string shortenedUrl = convertToBase(salt + encodedUrl, _validChars.Length);
        //TODO: save the shortUrl in db with long url, 

        return shortenedUrl;
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
}
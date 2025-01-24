namespace urlshortener.service;

public static class UrlShortener
{
    public static string ShortenUrl(string longUrl)
    {
        //TODO: Hash the longUrl. Create id, convert the ID to base x

        return "shorten-" + longUrl;
    }
}
using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore;
using Moq;
using StackExchange.Redis;
using System.Text.RegularExpressions;
using UrlShortenerResult = (ResultCode code, string shortUrl);
namespace urlshortener.service.tests;

public class UrlShortenerTests
{

    private Mock<Func<long>> _mockId;
    private Mock<DbSet<UrlMapping>> _mockUrlMappings;
    private Mock<UrlShortenerDbContext> _mockDb;

    private List<UrlMapping> _mockData;
    private const string NEW_URL = "http://localhost:8080/";
    private static KeyValuePair<string, string> URL_EXISTING =
        new KeyValuePair<string, string>("http://localhost:8080/existing", "abc123");
    private const string URL_ERROR = "http://localhost:8080/error";

    private Mock<IConnectionMultiplexer> _mockMux;
    private Mock<IDatabase> _mockRedis;

    [OneTimeSetUp]
    public void SetupMockGetId()
    {
        _mockId = new Mock<Func<long>>();
        _mockId.Setup(f => f()).Returns(1737787895940);

        //Mock the DbContext
        _mockData = new List<UrlMapping>()
        {
            new UrlMapping{Id = 1, LongUrl = URL_EXISTING.Key, ShortUrl = URL_EXISTING.Value},
        };

        var queryable = _mockData.AsQueryable();

        _mockUrlMappings = new Mock<DbSet<UrlMapping>>();
        _mockUrlMappings.Setup(u => u.Add(It.Is<UrlMapping>(u => u.LongUrl == URL_ERROR))).Throws(new Exception());
        _mockUrlMappings.Setup(u => u.Add(It.Is<UrlMapping>(u => u.LongUrl != URL_ERROR && u.LongUrl != NEW_URL)))
                        .Callback(_mockData.Add);
        //note: do nothing for NEW_URL so it always does not exist in the mockdata
        _mockUrlMappings.As<IQueryable<UrlMapping>>().Setup(u => u.Provider).Returns(queryable.Provider);
        _mockUrlMappings.As<IQueryable<UrlMapping>>().Setup(u => u.Expression).Returns(queryable.Expression);
        _mockUrlMappings.As<IQueryable<UrlMapping>>().Setup(u => u.ElementType).Returns(queryable.ElementType);
        _mockUrlMappings.As<IQueryable<UrlMapping>>().Setup(u => u.GetEnumerator()).Returns(queryable.GetEnumerator());

        //mock dbset Find
        _mockUrlMappings.Setup(u => u.Find(It.IsAny<object[]>()))
                        .Returns((object[] longUrlParams) =>
                         _mockData.FirstOrDefault(m => m.LongUrl == (string)longUrlParams[0]));

        _mockDb = new Mock<UrlShortenerDbContext>();
        _mockDb.Setup(db => db.UrlMappings).Returns(_mockUrlMappings.Object);

        var mockHash = new Dictionary<string, string>();

        _mockMux = new Mock<IConnectionMultiplexer>();
        _mockRedis = new Mock<IDatabase>();
        _mockRedis.Setup(r => r.HashSet("SHORTURL_MAPPING", It.IsAny<RedisValue>(), It.IsAny<RedisValue>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .Callback((RedisKey x, RedisValue field, RedisValue val, When y, CommandFlags z) =>
            {
                var k = (string?)field;
                var v = (string?)val;
                if (k == null || v == null)
                {
                    return;
                }
                mockHash[k] = v;
            });
        _mockRedis.Setup(r => r.HashGet("SHORTURL_MAPPING", It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .Returns((RedisKey x, RedisValue field, CommandFlags z) =>
            {
                var f = (string?)field;
                if (f == null)
                {
                    return RedisValue.Null;
                }
                return mockHash.ContainsKey(f) ? (RedisValue)mockHash[f] : RedisValue.Null;
            });

        _mockMux.Setup(mux => mux.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_mockRedis.Object);

    }

    [Test]
    [TestCaseSource(nameof(ShortenUrlGenerationTestCases))]
    public void ShortenUrl_ShouldReturnString_WhenIdAndUrlIsConvertedToBaseNfAllowedChars(string allowedChars, UrlShortenerResult expected)
    {
        var urlShortener = new UrlShortener(_mockId.Object, new Regex(allowedChars), _mockDb.Object, _mockMux.Object);
        var result = urlShortener.ShortenUrl(NEW_URL);
        Assert.That(result, Is.EqualTo(expected));
    }

    private static IEnumerable<TestCaseData> ShortenUrlGenerationTestCases()
    {
        var testCases = new List<TestCaseData>(){
            new TestCaseData(@"[a-zA-Z0-9]", (ResultCode.OK, "Uas8udI"))
                .SetName("ShortenURL should return string as a result of (id + hex encoded url) conversion to base n (n = number of allowed characters)"),
            new TestCaseData(@"[a-z]", (ResultCode.OK, "iijlnbdts"))
                .SetName("ShortenURL should return string as a result of (id + hex encoded url) conversion to base 26"),
        };
        foreach (var tc in testCases)
        {
            yield return tc;
        }
    }

    [Test]
    public void ShortenUrl_ShouldReturnADifferentStringPerMs()
    {
        var urlShortener = new UrlShortener(_mockDb.Object, _mockMux.Object);
        var shortUrl1 = urlShortener.ShortenUrl(NEW_URL);
        Assert.That(() => shortUrl1 != urlShortener.ShortenUrl(NEW_URL), Is.True.After(5).MilliSeconds);
    }

    [Test]
    [TestCaseSource(nameof(ShortenUrlCheckDbTestCases))]
    public void ShortenUrl_ShouldCheckDb(string longUrl, UrlShortenerResult expected)
    {
        var urlShortener = new UrlShortener(_mockId.Object, null, _mockDb.Object, _mockMux.Object);
        var result = urlShortener.ShortenUrl(longUrl);
        Assert.That(result, Is.EqualTo(expected));
    }

    private static IEnumerable<TestCaseData> ShortenUrlCheckDbTestCases()
    {
        var testCases = new List<TestCaseData>()
        {
            new TestCaseData(NEW_URL, (ResultCode.OK, "Uas8udI"))
                .SetName("If longUrl not existing in db, should return OK and its shortUrl"),
            new TestCaseData(URL_EXISTING.Key, (ResultCode.EXISTS, URL_EXISTING.Value))
                .SetName("If longUrl exists in db, should return EXISTS and its existing shortUrl"),
            new TestCaseData(URL_ERROR, (ResultCode.ERROR, string.Empty))
                .SetName("If adding url has exception, should return ERROR and empty short url"),

        };
        foreach (var tc in testCases)
        {
            yield return tc;
        }
    }

    [Test]
    [TestCaseSource(nameof(GetLongUrlDbTestCases))]
    public void GetLongUrl_ShouldCheckDb(string shortUrl, UrlShortenerResult expected)
    {
        var urlShortener = new UrlShortener(_mockDb.Object, _mockMux.Object);
        var result = urlShortener.GetLongUrl(shortUrl);
        Assert.That(result, Is.EqualTo(expected));
    }

    private static IEnumerable<TestCaseData> GetLongUrlDbTestCases()
    {
        var testCases = new List<TestCaseData>(){
            new TestCaseData(URL_EXISTING.Value, (ResultCode.OK, URL_EXISTING.Key))
            .SetName("If short url exists, return OK and long url"),
            new TestCaseData("x123", (ResultCode.NOT_FOUND, string.Empty))
            .SetName("If short url does not exists, return NOT_FOUND and empty string"),
        };
        foreach (var tc in testCases)
        {
            yield return tc;
        }
    }
}
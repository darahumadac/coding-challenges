using Microsoft.EntityFrameworkCore;
using Moq;
using System.Text.RegularExpressions;
using UrlShortenerResult = (ResultCode code, string shortUrl);
namespace urlshortener.service.tests;

public class UrlShortenerTests
{

    private Mock<Func<long>> _mockSalt;
    private Mock<DbSet<UrlMapping>> _mockUrlMappings;
    private Mock<UrlShortenerDbContext> _mockDb;
    private const string URL = "http://localhost:8080/";
    private const string URL_EXISTING = "http://localhost:8080/existing";
    [OneTimeSetUp]
    public void SetupMockGetId()
    {
        _mockSalt = new Mock<Func<long>>();
        _mockSalt.Setup(f => f()).Returns(1737787895940);

        //Mock the DbContext
        var mockData = new List<UrlMapping>()
        {
            new UrlMapping{Id = 1, LongUrl = URL_EXISTING, ShortUrl = "abc123"},
        }.AsQueryable();

        _mockUrlMappings = new Mock<DbSet<UrlMapping>>();
        _mockUrlMappings.As<IQueryable<UrlMapping>>().Setup(m => m.GetEnumerator()).Returns(mockData.GetEnumerator());

        _mockDb = new Mock<UrlShortenerDbContext>();
        _mockDb.Setup(db => db.UrlMappings).Returns(_mockUrlMappings.Object);

    }

    [Test]
    [TestCaseSource(nameof(ShortenUrlTestCases))]
    public void ShortenUrl_ShouldReturnString_WhenIdAndUrlIsConvertedToBaseNfAllowedChars(string allowedChars, UrlShortenerResult expected)
    {
        var urlShortener = new UrlShortener(_mockSalt.Object, new Regex(allowedChars), _mockDb.Object);
        var result = urlShortener.ShortenUrl(URL);
        Assert.That(result.shortUrl, Is.EqualTo(expected.shortUrl));
    }

    public static IEnumerable<TestCaseData> ShortenUrlTestCases()
    {
        yield return new TestCaseData(@"[a-zA-Z0-9]", (ResultCode.OK, "Uas8udI"))
        {
            TestName = "ShortenURL should return string as a result of (id + hex encoded url) conversion to base n (n = number of allowed characters)",
        };

        yield return new TestCaseData(@"[a-z]", (ResultCode.OK, "iijlnbdts"))
        {
            TestName = "ShortenURL should return string as a result of (id + hex encoded url) conversion to base 26",
        };

        //TODO: Add more testcases
    }


    [Test]
    public void ShortenUrl_ShouldReturnADifferentStringPerMs()
    {
        var urlShortener = new UrlShortener(_mockDb.Object);
        var shortUrl1 = urlShortener.ShortenUrl(URL);
        Assert.That(() => shortUrl1 != urlShortener.ShortenUrl(URL), Is.True.After(5).MilliSeconds);
    }
}
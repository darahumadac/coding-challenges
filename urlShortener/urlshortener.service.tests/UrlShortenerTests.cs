using Moq;
using System.Text.RegularExpressions;
namespace urlshortener.service.tests;

public class UrlShortenerTests
{

    private Mock<Func<long>> _mockSalt;
    private const string URL = "http://localhost:8080/";
    [OneTimeSetUp]
    public void SetupMockGetId()
    {
        _mockSalt = new Mock<Func<long>>();
        _mockSalt.Setup(f => f()).Returns(1737787895940);
    }

    [Test]
    [TestCase(
       @"[a-zA-Z0-9]",
       "UeWye0g",
       TestName = "ShortenURL should return string as a result of (id + hex encoded url) conversion to base n (n = number of allowed characters)"
    )]
    [TestCase(
       @"[a-z]",
       "iiuhpthve",
       TestName = "ShortenURL should return string as a result of (id + hex encoded url) conversion to base 26"
    )]
    public void ShortenUrl_ShouldReturnString_WhenIdAndUrlIsConvertedToBaseNfAllowedChars(string allowedCharPattern, string expectedShortUrl)
    {
        var urlShortener = new UrlShortener(_mockSalt.Object, new Regex(allowedCharPattern));
        var shortUrl = urlShortener.ShortenUrl(URL);
        Assert.That(shortUrl, Is.EqualTo(expectedShortUrl));
    }

    [Test]
    public void ShortenUrl_ShouldReturnADifferentStringPerMs()
    {
        var urlShortener = new UrlShortener();
        var shortUrl1 = urlShortener.ShortenUrl(URL);
        Assert.That(() => shortUrl1 != urlShortener.ShortenUrl(URL), Is.True.After(5).MilliSeconds);
    }
}
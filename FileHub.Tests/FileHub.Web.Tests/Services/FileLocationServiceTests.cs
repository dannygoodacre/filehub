using FileHub.Web.Configuration;
using FileHub.Web.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace FileHub.Web.Tests.Services;

[TestFixture]
public class FileLocationServiceTests : TestBase
{
    private Mock<IOptions<WebOptions>> _mockOptions;

    private FileLocationService _service;

    [SetUp]
    public void SetUp()
    {
        _mockOptions = new Mock<IOptions<WebOptions>>(MockBehavior.Strict);

        _service = new FileLocationService(_mockOptions.Object);
    }

    [Test]
    public void GetFileAccessLocation()
    {
        // Arrange
        const string fileId = "123";

        _mockOptions
            .Setup(m => m.Value)
            .Returns(new WebOptions()
            {
                RootUrl = "test_root_url",
            })
            .Verifiable(Times.Once);

        // Act
        var result = _service.GetFileAccessLocation(fileId);

        // Assert
        Assert.That(result, Is.EqualTo($"test_root_url/files/123"));
    }
}

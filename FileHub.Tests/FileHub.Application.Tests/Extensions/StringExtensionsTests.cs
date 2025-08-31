using FileHub.Application.Extensions;

namespace FileHub.Application.Tests.Extensions;

[TestFixture]
public class StringExtensionsTests : TestBase
{
    [TestCase("valid/mime_type", true)]
    [TestCase("valid/mime-type", true)]
    [TestCase("valid/mime-type+parameter", true)]
    [TestCase("valid/mime.type", true)]
    [TestCase("valid/mime.type+parameter", true)]
    [TestCase("valid/mime+type", true)]
    [TestCase("invalid_mime_type", false)]
    [TestCase("invalid/mime_type!", false)]
    [TestCase("invalid/@mime.type", false)]
    [TestCase("/invalid_mime.type", false)]
    public void IsValidMimeType(string mimeType, bool isValid)
    {
        // Act
        var result = mimeType.IsValidMimeType();

        // Assert
        Assert.That(result, Is.EqualTo(isValid));
    }
}

using Sqids;

namespace FileHub.Utilities.Tests;

[TestFixture]
public class IdEncoderServiceTests : TestBase
{
    private readonly IdEncoderService<int> _service = new(new SqidsEncoder<int>(
    new SqidsOptions
    {
        Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
        MinLength = 3
    }));

    [Test]
    public void Encode_WhenNegative_ReturnsNull()
    {
        // Arrange
        const int id = -1;

        // Act
        var result = _service.Encode(id);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Encode_WhenValid_ReturnsValidString()
    {
        // Arrange
        const int id = 123;

        // Act
        var result = _service.Encode(id);

        // Assert
        Assert.That(result, Is.Not.Empty);
        Assert.That(result?.Length, Is.GreaterThanOrEqualTo(3));
    }

    [TestCase(null!)]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("-")]
    [TestCase("123")]
    [TestCase("abc123")]
    public void Decode_WhenInvalidString_ReturnsZero(string id)
    {
        // Act
        var result = _service.Decode(id);

        // Assert
        Assert.That(result, Is.Zero);
    }

    [TestCase("ABC")]
    [TestCase("QWERTY")]
    public void Decode_WhenValid_ReturnsValidInteger(string id)
    {
        // Act
        var result = _service.Decode(id);

        // Assert
        Assert.That(result, Is.Not.Zero);
    }
}

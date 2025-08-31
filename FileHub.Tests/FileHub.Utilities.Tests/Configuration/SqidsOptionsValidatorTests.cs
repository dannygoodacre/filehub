using FileHub.Utilities.Configuration;
using Sqids;

namespace FileHub.Utilities.Tests.Configuration;

[TestFixture]
public class SqidsOptionsValidatorTests : TestBase
{
    private readonly SqidsOptionsValidator _validator = new();

    [Test]
    public void Validate_WhenMinLengthNegative_ShouldReturnFailure()
    {
        // Arrange
        var options = new SqidsOptions()
        {
            MinLength = -1
        };

        // Act
        var result = _validator.Validate("Test", options);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(result.Failures, Has.Count.EqualTo(1));
            Assert.That(result.Failures?.First(), Is.EqualTo($"'{nameof(options.MinLength)}' must be greater than or equal to 0."));
        }
    }

    [Test]
    public void Validate_WhenMinLengthGreaterThan255_ShouldReturnFailure()
    {
        // Arrange
        var options = new SqidsOptions()
        {
            MinLength = 256
        };

        // Act
        var result = _validator.Validate("Test", options);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(result.Failures, Has.Count.EqualTo(1));
            Assert.That(result.Failures?.First(), Is.EqualTo($"'{nameof(options.MinLength)}' must be less than or equal to 255."));
        }
    }

    [Test]
    public void Validate_WhenAlphabetLengthLessThan5_ShouldReturnFailure()
    {
        // Arrange
        var options = new SqidsOptions()
        {
            Alphabet = "ABCD"
        };

        // Act
        var result = _validator.Validate("Test", options);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(result.Failures, Has.Count.EqualTo(1));
            Assert.That(result.Failures?.First(), Is.EqualTo($"'{nameof(options.Alphabet)}' must be at least 5 characters."));
        }
    }

    [Test]
    public void Validate_WhenValid_ShouldReturnSuccess()
    {
        // Arrange
        var options = new SqidsOptions()
        {
            Alphabet = "ABCDEGHIJKL",
            MinLength = 10
        };

        // Act
        var result = _validator.Validate("Test", options);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Failed, Is.False);
            Assert.That(result.Failures, Is.Null);
        }
    }
}

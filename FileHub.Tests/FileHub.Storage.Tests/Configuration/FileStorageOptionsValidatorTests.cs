using FileHub.Storage.Configuration;

namespace FileHub.Storage.Tests.Configuration;

[TestFixture]
public class FileStorageOptionsValidatorTests : TestBase
{
    private readonly FileStorageOptionsValidator _validator = new();

    private string _tempDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(_tempDirectory, true);
    }

    [TestCase(-1)]
    [TestCase(0)]
    public void Validate_WhenFileStreamBufferSizeInvalid_ShouldReturnFailure(int bufferSize)
    {
        // Arrange
        var options = new FileStorageOptions
        {
            ContentDirectory = _tempDirectory,
            FileStreamBufferSize = bufferSize,
        };

        // Act
        var result = _validator.Validate("Test", options);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(result.Failures?.ToList(), Has.Count.EqualTo(1));
            Assert.That(result.Failures?.First(), Is.EqualTo($"'{nameof(options.FileStreamBufferSize)}' must be greater than 0."));
        }
    }

    [TestCase(null!)]
    [TestCase("")]
    [TestCase(" ")]
    public void Validate_WhenContentDirectoryInvalid_ShouldReturnFailure(string contentDirectory)
    {
        // Arrange
        var options = new FileStorageOptions
        {
            ContentDirectory = contentDirectory,
            FileStreamBufferSize = 123,
        };

        // Act
        var result = _validator.Validate("Test", options);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(result.Failures?.ToList(), Has.Count.EqualTo(1));
            Assert.That(result.Failures?.First(), Is.EqualTo($"'{nameof(options.ContentDirectory)}' must not be null, empty, or whitespace."));
        }
    }

    [Test]
    public void Validate_WhenContentDirectoryIsRelativeAndDoesntExist_ShouldReturnFailure()
    {
        // Arrange
        var options = new FileStorageOptions
        {
            ContentDirectory = "foo/bar",
            FileStreamBufferSize = 1234,
        };

        // Act
        var result = _validator.Validate("Test", options);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(result.Failures?.ToList(), Has.Count.EqualTo(1));
            Assert.That(result.Failures?.First(), Is.EqualTo($"'{nameof(options.ContentDirectory)}' does not exist."));
        };
    }

    [Test]
    public void Validate_WhenContentDirectoryDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var nonExistentDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        var options = new FileStorageOptions
        {
            ContentDirectory = nonExistentDirectory,
        };

        // Act
        var result = _validator.Validate("Test", options);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(result.Failures?.ToList(), Has.Count.EqualTo(1));
            Assert.That(result.Failures?.First(), Is.EqualTo($"'{nameof(options.ContentDirectory)}' does not exist."));
        };
    }

    [Test]
    public void Validate_WhenMultipleOptionsInvalid_ShouldReturnFailure()
    {
        // Arrange
        var options = new FileStorageOptions
        {
            ContentDirectory = "",
            FileStreamBufferSize = -1,
        };

        // Act
        var result = _validator.Validate("Test", options);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(result.Failures?.ToList(), Has.Count.EqualTo(2));
            Assert.That(result.Failures?.First(), Is.EqualTo($"'{nameof(options.FileStreamBufferSize)}' must be greater than 0."));
            Assert.That(result.Failures?.Skip(1).First(), Is.EqualTo($"'{nameof(options.ContentDirectory)}' must not be null, empty, or whitespace."));
        }
    }

    [Test]
    public void Validate_WhenAllOptionsValid_ShouldReturnSuccess()
    {
        // Arrange
        var options = new FileStorageOptions
        {
            ContentDirectory = _tempDirectory,
            FileStreamBufferSize = 1234,
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

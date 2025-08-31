using System.Text;
using System.Text.RegularExpressions;
using FileHub.Core.Common;
using FileHub.Storage.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FileHub.Storage.Tests;

[TestFixture]
public class FileStorageServiceTests : TestBase
{
    private string _testRootDirectoryPath = null!;

    private string _testDirectoryPath = null!;

    private string _testNonExistentDirectoryPath = null!;

    private string _testFilePath = null!;

    private string _testNonExistentFilePath = null!;

    private const string TestFileContent = "Test file content";

    private readonly byte[] _testFileContentBytes = Encoding.UTF8.GetBytes(TestFileContent);

    private const string TestFileExtension = ".txt";

    private CancellationToken _cancellationToken;

    private Mock<IOptions<FileStorageOptions>> _mockOptions = null!;

    private Mock<ILogger<FileStorageService>> _mockLogger = null!;

    private FileStorageService _fileStorageService = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _testRootDirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        Directory.CreateDirectory(_testRootDirectoryPath);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        if (Directory.Exists(_testRootDirectoryPath))
        {
            Directory.Delete(_testRootDirectoryPath, true);
        }
    }

    [SetUp]
    public async Task SetUp()
    {
        _mockOptions = new Mock<IOptions<FileStorageOptions>>(MockBehavior.Strict);

        _mockLogger = new Mock<ILogger<FileStorageService>>(MockBehavior.Strict);

        _fileStorageService = new FileStorageService(_mockLogger.Object, _mockOptions.Object);

        _testDirectoryPath = Path.Combine(_testRootDirectoryPath, Guid.NewGuid().ToString());

        Directory.CreateDirectory(_testDirectoryPath);

        _testNonExistentDirectoryPath = Path.Combine(_testRootDirectoryPath, Guid.NewGuid().ToString());

        _testFilePath = Path.Combine(_testDirectoryPath, Guid.NewGuid().ToString());

        await File.WriteAllBytesAsync(_testFilePath, _testFileContentBytes);

        _testNonExistentFilePath = Path.Combine(_testNonExistentDirectoryPath, Guid.NewGuid().ToString());

        _cancellationToken = CancellationToken.None;
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectoryPath))
        {
            Directory.Delete(_testDirectoryPath, true);
        }
    }

    [Test]
    public async Task SaveAsync_WhenDirectoryDoesNotExist_ShouldReturnError()
    {
        // Arrange
        _mockOptions
            .Setup(x => x.Value)
            .Returns(new FileStorageOptions()
            {
                ContentDirectory = _testNonExistentDirectoryPath,
            })
            .Verifiable(Times.Once);

        _mockLogger.Setup(
                LogLevel.Error,
                "Error while saving file.",
                exception: new DirectoryNotFoundException($"Could not find a part of the path '{_testNonExistentDirectoryPath}"),
                verifyContainsExceptionMessage: true);

        var fileStream = new MemoryStream(_testFileContentBytes);

        // Act
        var result = await _fileStorageService.SaveAsync(fileStream, TestFileExtension, _cancellationToken);

        // Assert
       Assert.That(result.IsSuccess, Is.False);
       Assert.That(result.Status, Is.EqualTo(Status.InternalError));
    }

    [Test]
    public async Task SaveAsync_WhenSuccess_ShouldReturnPath()
    {
        // Arrange
        SetupOptions();

        var fileStream = new MemoryStream(_testFileContentBytes);

        // Act
        var result = await _fileStorageService.SaveAsync(fileStream, TestFileExtension, _cancellationToken);

        // Assert
        var today = DateTime.UtcNow.ToString("yyyyMMdd");

        var escapedDirectoryPath = Regex.Escape(_testDirectoryPath);

        var pattern = $"^{escapedDirectoryPath}/{today}_[0-9a-f]{{8}}-[0-9a-f]{{4}}-[0-9a-f]{{4}}-[0-9a-f]{{4}}-[0-9a-f]{{12}}{TestFileExtension}$";

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Does.Match(pattern));
        }
    }

    [Test]
    public async Task OpenReadStreamAsync_WhenFileDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        SetupOptions();

        _mockLogger.Setup(
            LogLevel.Error,
            $"Error while opening file at Path '{_testNonExistentFilePath}'.",
            exception: new DirectoryNotFoundException($"Could not find a part of the path '{_testNonExistentFilePath}'."));

        // Act
        var result = await _fileStorageService.OpenReadStreamAsync(_testNonExistentFilePath, _cancellationToken);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo(Status.InternalError));
        }
    }

    [Test]
    public async Task OpenReadStreamAsync_WhenSuccess_ShouldReturnStream()
    {
        // Arrange
        SetupOptions();

        // Act
        var result = await _fileStorageService.OpenReadStreamAsync(_testFilePath, _cancellationToken);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
        }

        string content;

        using (var reader = new StreamReader(result.Value!))
        {
            content = await reader.ReadToEndAsync();
        }

        Assert.That(content, Is.EqualTo(TestFileContent));
    }

    private void SetupOptions(int times = 1)
    {
        _mockOptions.Setup(x => x.Value)
            .Returns(new FileStorageOptions()
            {
                ContentDirectory = _testDirectoryPath,
                FileStreamBufferSize = 4096
            })
            .Verifiable(Times.Exactly(times));
    }
}

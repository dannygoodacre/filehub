using FileHub.Application.Abstractions.Data;
using FileHub.Application.Abstractions.Data.Repositories;
using FileHub.Application.Abstractions.Services;
using FileHub.Application.Commands;
using FileHub.Core.Common;
using FileHub.Core.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace FileHub.Application.Tests.Commands;

[TestFixture]
public class AddFileTests : TestBase
{
    private const string Name = "Add File";

    private Mock<ILogger<AddFile>> _mockLogger;

    private Mock<IFileRepository> _mockFileRepository;

    private Mock<ITagRepository> _mockTagRepository;

    private Mock<IFileStorageService> _mockFileStorageService;

    private Mock<IApplicationContext> _mockApplicationContext;

    private AddFile _command;

    private Stream _requestContent;

    private string _requestContentType;

    private string _requestOriginalFileName;

    private string _requestName;

    private int _requestUserId;

    private List<string> _requestTags;

    private List<Tag> _testTags;

    private int _testActualChanges;

    private CancellationToken _cancellationToken;

    private const string TestExtension = ".ext";

    private const string TestStorageKey = "Test Storage Key";

    private DateTime _timeBeforeTest;

    [SetUp]
    public void SetUp()
    {
        _requestContent = new MemoryStream("Test Content"u8.ToArray());

        _requestContentType = "test/content-type";

        _requestOriginalFileName = "test_original_filename.ext";

        _requestName = "Test Name";

        _requestUserId = 123;

        _requestTags = ["Test Tag 1", "Test Tag 2"];

        _testTags = [new Tag { Name = "Test Tag 1" }, new Tag { Name = "Test Tag 2" }];

        _testActualChanges = 1;

        _cancellationToken = CancellationToken.None;

        _timeBeforeTest = DateTime.UtcNow;

        _mockLogger = new Mock<ILogger<AddFile>>(MockBehavior.Strict);

        _mockFileRepository = new Mock<IFileRepository>(MockBehavior.Strict);

        _mockTagRepository = new Mock<ITagRepository>(MockBehavior.Strict);

        _mockFileStorageService = new Mock<IFileStorageService>(MockBehavior.Strict);

        _mockApplicationContext = new Mock<IApplicationContext>(MockBehavior.Strict);

        _command = new AddFile(_mockLogger.Object,
                               _mockFileRepository.Object,
                               _mockTagRepository.Object,
                               _mockFileStorageService.Object,
                               _mockApplicationContext.Object);
    }

    [Test]
    public async Task ExecuteAsync_WhenContentEmpty_ShouldReturnInvalid()
    {
        // Arrange
        _requestContent = new MemoryStream();

        _mockLogger.Setup(LogLevel.Error, $"Command '{Name}' failed validation: Content:{Environment.NewLine}  - Must not be empty.");

        // Act
        var result = await Act();

        // Assert
        AssertInvalid(result);
    }

    [Test]
    public async Task ExecuteAsync_WhenContentTypeInvalid_ShouldReturnInvalid()
    {
        // Arrange
        _requestContentType = "Invalid MIME Type";

        _mockLogger.Setup(LogLevel.Error, $"Command '{Name}' failed validation: ContentType:{Environment.NewLine}  - Must be a valid MIME type.");

        // Act
        var result = await Act();

        // Assert
        AssertInvalid(result);
    }

    [TestCase(null!)]
    [TestCase("")]
    [TestCase(" ")]
    public async Task ExecuteAsync_WhenOriginalFileNameInvalid_ShouldReturnInvalid(string originalFileName)
    {
        // Arrange
        _requestOriginalFileName = originalFileName;

        _mockLogger.Setup(LogLevel.Error, $"Command '{Name}' failed validation: OriginalFileName:{Environment.NewLine}  - Must not be null or whitespace.");

        // Act
        var result = await Act();

        // Assert
        AssertInvalid(result);
    }

    [TestCase(null!)]
    [TestCase("")]
    [TestCase(" ")]
    public async Task ExecuteAsync_WhenNameInvalid_ShouldReturnInvalid(string name)
    {
        // Arrange
        _requestName = name;

        _mockLogger.Setup(LogLevel.Error, $"Command '{Name}' failed validation: Name:{Environment.NewLine}  - Must not be null or whitespace.");

        // Act
        var result = await Act();

        // Assert
        AssertInvalid(result);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public async Task ExecuteAsync_WhenUserIdInvalid_ShouldReturnInvalid(int userId)
    {
        // Arrange
        _requestUserId = userId;

        _mockLogger.Setup(LogLevel.Error, $"Command '{Name}' failed validation: UserId:{Environment.NewLine}  - Must be greater than 0.");

        // Act
        var result = await Act();

        // Assert
        AssertInvalid(result);
    }

    [Test]
    public async Task ExecuteAsync_WhenTagsWhitespace_ShouldReturnInvalid()
    {
        // Arrange
        _requestTags = ["Test tag 1", " "];

        _mockLogger.Setup(LogLevel.Error, $"Command '{Name}' failed validation: Tags:{Environment.NewLine}  - Must not be null or whitespace.");

        // Act
        var result = await Act();

        // Assert
        AssertInvalid(result);
    }

    [Test]
    public async Task ExecuteAsync_WhenExtensionNotFound_ShouldReturnFailed()
    {
        // Arrange
        _requestOriginalFileName = "filename_without_extension";

        Setup_Logger_Starting();

        _mockLogger.Setup(LogLevel.Error, $"Command '{Name}' could not determine the extension for File '{_requestOriginalFileName}'.");

        // Act
        var result = await Act();

        // Assert
        AssertFailed(result, "Could not find file extension.");
    }

    [Test]
    public async Task ExecuteAsync_WhenStorageServiceFails_ShouldReturnFailed()
    {
        // Arrange
        Setup_Logger_Starting();

        _mockLogger.Setup(LogLevel.Error, $"Command '{Name}' could not save File '{_requestOriginalFileName}'.");

        const string errorMessage = "Test storage error message.";

        _mockFileStorageService
            .Setup(x => x.SaveAsync(
                It.Is<Stream>(y => y == _requestContent),
                It.Is<string>(y => y == TestExtension),
                It.Is<CancellationToken>(y => y == _cancellationToken)))
            .ReturnsAsync(Result<string>.Failed(errorMessage))
            .Verifiable(Times.Once);

        // Act
        var result = await Act();

        // Assert
        AssertFailed(result, errorMessage);
    }

    [Test]
    public async Task ExecuteAsync_WhenTagsNull_ShouldReturnSuccess()
    {
        // Arrange
        _requestTags = null!;
        _testTags = [];

        Setup_Logger_Starting();

        Setup_FileStorageService_SaveAsync();

        Setup_FileRepository_Add();

        Setup_ApplicationContext_SaveChangesAsync();

        Setup_Logger_Completed();

        // Act
        var result = await Act();

        // Assert
        AssertSuccess(result);
    }

    [Test]
    public async Task ExecuteAsync_WhenTagsEmpty_ShouldReturnSuccess()
    {
        // Arrange
        _requestTags = [];
        _testTags = [];

        Setup_Logger_Starting();

        Setup_FileStorageService_SaveAsync();

        Setup_FileRepository_Add();

        Setup_ApplicationContext_SaveChangesAsync();

        Setup_Logger_Completed();

        // Act
        var result = await Act();

        // Assert
        AssertSuccess(result);
    }

    [Test]
    public async Task ExecuteAsync_WhenAllTagsAlreadyExist_ShouldReturnSuccess()
    {
        // Arrange
        _testActualChanges = 3;

        Setup_Logger_Starting();

        Setup_FileStorageService_SaveAsync();

        Setup_TagRepository_GetManyAsync();

        Setup_FileRepository_Add();

        Setup_ApplicationContext_SaveChangesAsync();

        Setup_Logger_Completed();

        // Act
        var result = await Act();

        // Assert
        AssertSuccess(result);
    }

    [Test]
    public async Task ExecuteAsync_WhenNewTags_ShouldReturnSuccess()
    {
        // Arrange
        _testActualChanges = 6;
        _requestTags = ["Test Tag 1", "Test Tag 2", "Test Tag 3"];
        _testTags = [new Tag { Name = "Test Tag 2" }];

        Setup_Logger_Starting();

        Setup_FileStorageService_SaveAsync();

        Setup_TagRepository_GetManyAsync();

        _mockTagRepository
            .Setup(x => x.AddRange(
                It.Is<List<Tag>>(y => y.Count == 2
                                 && y[0].Name == "Test Tag 1"
                                 && y[1].Name == "Test Tag 3")))
            .Verifiable(Times.Once);

        Setup_FileRepository_Add();

        Setup_ApplicationContext_SaveChangesAsync();

        Setup_Logger_Completed();

        // Act
        var result = await Act();

        // Assert
        AssertSuccess(result);
    }

    [Test]
    public async Task ExecuteAsync_WhenUnexpectedNumberOfSavedChanges_ShouldReturnSuccess()
    {
        // Arrange
        _requestTags = null!;
        _testTags = [];

        Setup_Logger_Starting();

        Setup_FileStorageService_SaveAsync();

        Setup_FileRepository_Add();

        _mockApplicationContext
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(2));

        _mockLogger.Setup(LogLevel.Error, $"Command '{Name}' wrote an unexpected number of entities to the database for File '{_requestOriginalFileName}': expected '1', actual '2'.");

        Setup_Logger_Completed();

        // Act
        var result = await Act();

        // Assert
        AssertSuccess(result);
    }

    private async Task<Result> Act() => await _command.ExecuteAsync(_requestContent,
                                                                    _requestContentType,
                                                                    _requestOriginalFileName,
                                                                    _requestName,
                                                                    _requestUserId,
                                                                    _requestTags,
                                                                    _cancellationToken);

    private void Setup_Logger_Starting()
    {
        _mockLogger.Setup(LogLevel.Information, $"Command '{Name}' started for File '{_requestOriginalFileName}', Name '{_requestName}', User '{_requestUserId}'.");
    }

    private void Setup_Logger_Completed()
    {
        _mockLogger.Setup(LogLevel.Information, $"Command '{Name}' completed for File '{_requestOriginalFileName}', Name '{_requestName}'.");
    }

    private void Setup_FileStorageService_SaveAsync(int times = 1)
    {
        _mockFileStorageService
            .Setup(x => x.SaveAsync(
                It.Is<Stream>(y => y == _requestContent),
                It.Is<string>(y => y == TestExtension),
                It.Is<CancellationToken>(y => y == _cancellationToken)))
            .ReturnsAsync(Result<string>.Success(TestStorageKey))
            .Verifiable(Times.Exactly(times));
    }

    private void Setup_FileRepository_Add(int times = 1)
    {
        _mockFileRepository
            .Setup(x => x.Add(
                It.Is<StoredFile>(y => y.Name == _requestName
                                       && y.StorageKey == TestStorageKey
                                       && y.ContentType == _requestContentType
                                       && y.CreatedAt >= _timeBeforeTest
                                       && y.Tags.SequenceEqual(_testTags)
                                       && y.UserId == _requestUserId)))
            .Verifiable(Times.Exactly(times));
    }

    private void Setup_ApplicationContext_SaveChangesAsync(int times = 1)
    {
        _mockApplicationContext
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(_testActualChanges))
            .Verifiable(Times.Exactly(times));
    }

    private void Setup_TagRepository_GetManyAsync(int times = 1)
    {
        _mockTagRepository
            .Setup(x => x.GetManyForUpdateAsync(
                It.Is<List<string>>(y => y == _requestTags),
                It.Is<CancellationToken>(y => y == _cancellationToken)))
            .ReturnsAsync(_testTags)
            .Verifiable(Times.Exactly(times));
    }
}

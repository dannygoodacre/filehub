using FileHub.Application.Abstractions.Data.Repositories;
using FileHub.Application.Abstractions.Services;
using FileHub.Application.Queries;
using FileHub.Core.Common;
using FileHub.Core.Entities;
using FileHub.Core.Files;
using Microsoft.Extensions.Logging;
using Moq;

namespace FileHub.Application.Tests.Queries;

[TestFixture]
public class GetFileContentTests : TestBase
{
    private const string Name = "Get File Content";

    private Mock<ILogger<GetFileContent>> _mockLogger = null!;

    private Mock<IFileRepository> _mockRepository = null!;

    private Mock<IFileStorageService> _mockStorageService = null!;

    private Mock<IIdEncoderService<int>> _mockIdEncoderService = null!;

    private GetFileContent _query;

    private string _requestId;

    private int _testId;

    private CancellationToken _testCancellationToken;

    private StoredFile _testStoredFile = null!;

    private const string TestName = "Test File Name";

    private const string TestStorageKey = "Test Storage Key";

    private const string TestContentType = "Test Content Type";

    private readonly DateTime _testCreatedAt = new DateTime(2020, 10, 12);

    private readonly List<Tag> _testTags =
    [
        new() { Id = 456, Name = "Test Tag 1" },
        new() { Id = 789, Name = "Test Tag 2" }
    ];

    private const int TestUserId = 101;

    private Stream _testStream = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<GetFileContent>>(MockBehavior.Strict);

        _mockRepository = new Mock<IFileRepository>(MockBehavior.Strict);

        _mockStorageService = new Mock<IFileStorageService>(MockBehavior.Strict);

        _mockIdEncoderService = new Mock<IIdEncoderService<int>>(MockBehavior.Strict);

        _query = new GetFileContent(_mockLogger.Object,
                                    _mockRepository.Object,
                                    _mockStorageService.Object,
                                    _mockIdEncoderService.Object);

        _testId = 123;

        _testCancellationToken = CancellationToken.None;

        _testStoredFile = new StoredFile
        {
            Id = _testId,
            Name = TestName,
            StorageKey = TestStorageKey,
            ContentType = TestContentType,
            CreatedAt = _testCreatedAt,
            Tags = _testTags,
            UserId = TestUserId,
        };

        _testStream = new MemoryStream();

        _requestId = "Test External Id";
    }

    [TestCase(null!)]
    [TestCase("")]
    [TestCase(" ")]
    public async Task ExecuteAsync_WhenIdNull_ShouldReturnInvalid(string id)
    {
        // Arrange
        _requestId = id;

        _mockLogger.Setup(LogLevel.Error, $"Query '{Name}' failed validation: Id:{Environment.NewLine}  - Must not be null, empty, or whitespace.");

        // Act
        var result = await Act();

        // Assert
        AssertInvalid(result);
    }

    [Test]
    public async Task ExecuteAsync_WhenIdDecodingFails_ShouldReturnNotFound()
    {
        // Arrange
        Setup_Logger_Starting();

        _testId = 0;

        Setup_IdEncoderService_Decode();

        _mockLogger.Setup(LogLevel.Error, $"Query '{Name}' could not decode the external ID '{_requestId}'.");

        // Act
        var result = await Act();

        // Assert
        AssertNotFound(result);
    }

    [Test]
    public async Task ExecuteAsync_WhenRepositoryReturnsNull_ShouldReturnNotFound()
    {
        // Arrange
        Setup_Logger_Starting();

        Setup_IdEncoderService_Decode();

        _testStoredFile = null!;

        Setup_FileRepository_GetByIdAsync();

        _mockLogger.Setup(LogLevel.Error, $"Query '{Name}' could not find a file with ID '{_testId}'.");

        // Act
        var result = await Act();

        // Assert
        AssertNotFound(result);
    }

    [Test]
    public async Task ExecuteAsync_WhenStorageServiceFails_ShouldReturnFailed()
    {
        // Arrange
        Setup_Logger_Starting();

        Setup_IdEncoderService_Decode();

        Setup_FileRepository_GetByIdAsync();

        const string failureMessage = "Test Failure Message";

        _mockStorageService
            .Setup(x => x.OpenReadStreamAsync(
                It.Is<string>(y => y == TestStorageKey),
                It.Is<CancellationToken>(y => y == _testCancellationToken)))
            .ReturnsAsync(Result<Stream>.Failed(failureMessage))
            .Verifiable(Times.Once);

        _mockLogger.Setup(LogLevel.Error, $"Query '{Name}' could not read the file with ID '{_testId}' and Storage Key '{TestStorageKey}'.");

        // Act
        var result = await Act();

        // Assert
        AssertFailed(result, failureMessage);
    }

    [Test]
    public async Task ExecuteAsync_WhenContentResultIsNull_ShouldReturnFailed()
    {
        // Arrange
        Setup_Logger_Starting();

        Setup_IdEncoderService_Decode();

        Setup_FileRepository_GetByIdAsync();

        _testStream = null!;

        Setup_FileStorageService_OpenReadStreamAsync();

        _mockLogger.Setup(LogLevel.Error, $"Query '{Name}' could not read the file with ID '{_testId}' and Storage Key '{TestStorageKey}'.");

        // Act
        var result = await Act();

        // Assert
        AssertFailed(result, "File content is empty.");
    }

    [Test]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnSuccess()
    {
        // Arrange
        Setup_Logger_Starting();

        Setup_IdEncoderService_Decode();

        Setup_FileRepository_GetByIdAsync();

        Setup_FileStorageService_OpenReadStreamAsync();

        _mockLogger.Setup(LogLevel.Information, $"Query '{Name}' completed for external ID '{_requestId}', ID '{_testId}'.");

        // Act
        var result = await Act();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            AssertSuccess(result);

            Assert.That(result.Value?.Content, Is.EqualTo(_testStream));
            Assert.That(result.Value?.ContentType, Is.EqualTo(TestContentType));
        }
    }

    private async Task<Result<FileContent>> Act() => await _query.ExecuteAsync(_requestId, _testCancellationToken);

    private void Setup_Logger_Starting()
    {
        _mockLogger.Setup(LogLevel.Information, $"Query '{Name}' started with external ID '{_requestId}'.");
    }

    private void Setup_FileRepository_GetByIdAsync(int times = 1)
    {
        _mockRepository
            .Setup(x => x.GetByIdAsync(
                It.Is<int>(y => y == _testId),
                It.Is<CancellationToken>(y => y == _testCancellationToken)))
            .ReturnsAsync(_testStoredFile)
            .Verifiable(Times.Exactly(times));
    }

    private void Setup_FileStorageService_OpenReadStreamAsync(int times = 1)
    {
        _mockStorageService
            .Setup(x => x.OpenReadStreamAsync(
                It.Is<string>(y => y == TestStorageKey),
                It.Is<CancellationToken>(y => y == _testCancellationToken)))
            .ReturnsAsync(Result<Stream>.Success(_testStream))
            .Verifiable(Times.Exactly(times));
    }

    private void Setup_IdEncoderService_Decode(int times = 1)
    {
        _mockIdEncoderService
            .Setup(x => x.Decode(
                It.Is<string>(y => y == _requestId)))
            .Returns(_testId)
            .Verifiable(Times.Exactly(times));
    }
}

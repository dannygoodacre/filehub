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
public class GetFileMetadataTests : TestBase
{
    private const string Name = "Get File Metadata";

    private Mock<ILogger<GetFileMetadata>> _mockLogger = null!;

    private Mock<IFileRepository> _mockRepository = null!;

    private Mock<IFileLocationService> _mockFileLocationService = null!;

    private Mock<IIdEncoderService<int>> _mockIdEncoderService = null!;

    private GetFileMetadata _query;

    private string _requestId = null!;

    private int _testId = 123;

    private CancellationToken _testCancellationToken;

    private StoredFile _testStoredFile = null!;

    private const string TestName = "Test File Name";

    private const string TestStorageKey = "Test Storage Key";

    private const string TestContentType = "Test Content Type";

    private readonly DateTime _testCreatedAt = new DateTime(2020, 10, 12);

    private readonly Category _testCategory = new()
    {
        Id = 123,
        Name = "Test Category"
    };

    private readonly List<Tag> _testTags =
    [
        new() { Id = 456, Name = "Test Tag 1" },
        new() { Id = 789, Name = "Test Tag 2" }
    ];

    private const int TestUserId = 101;

    private const string TestAccessLocation = "Test Access Location";

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<GetFileMetadata>>(MockBehavior.Strict);

        _mockRepository = new Mock<IFileRepository>(MockBehavior.Strict);

        _mockFileLocationService = new Mock<IFileLocationService>(MockBehavior.Strict);

        _mockIdEncoderService = new Mock<IIdEncoderService<int>>(MockBehavior.Strict);

        _query = new GetFileMetadata(_mockLogger.Object, _mockRepository.Object, _mockFileLocationService.Object, _mockIdEncoderService.Object);

        _requestId = "Request Id";

        _testCancellationToken = CancellationToken.None;

        _testStoredFile = new StoredFile
        {
            Id = _testId,
            Name = TestName,
            StorageKey = TestStorageKey,
            ContentType = TestContentType,
            CreatedAt = _testCreatedAt,
            Category = _testCategory,
            Tags = _testTags,
            UserId = TestUserId,
        };
    }

    [TestCase(null!)]
    [TestCase("")]
    [TestCase(" ")]
    public async Task ExecuteAsync_WhenIdInvalid_ShouldReturnInvalid(string id)
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
    public async Task ExecuteAsync_WhenFileNotFound_ShouldReturnNotFound()
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
    public async Task ExecuteAsync_WhenFileFound_ShouldReturnSuccess()
    {
        // Arrange
        Setup_Logger_Starting();

        Setup_IdEncoderService_Decode();

        Setup_FileRepository_GetByIdAsync();

        Setup_FileLocationService_GetFileAccessLocation();

        // Act
        var result = await Act();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            AssertSuccess(result);

            Assert.That(result.Value?.Name, Is.EqualTo(TestName));
            Assert.That(result.Value?.AccessLocation, Is.EqualTo(TestAccessLocation));
            Assert.That(result.Value?.ContentType, Is.EquivalentTo(TestContentType));
            Assert.That(result.Value?.CreatedAt, Is.EqualTo(_testCreatedAt));
            Assert.That(result.Value?.Tags, Is.EquivalentTo(_testTags.Select(x => x.Name)));
        }
    }

    private async Task<Result<FileMetadata>> Act() => await _query.ExecuteAsync(_requestId, _testCancellationToken);

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

    private void Setup_FileLocationService_GetFileAccessLocation(int times = 1)
    {
        _mockFileLocationService
            .Setup(x => x.GetFileAccessLocation(
                It.Is<string>(y => y == _requestId)))
            .Returns(TestAccessLocation)
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

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
public class GetPaginatedFilesMetadataTests : TestBase
{
    private const string Name = "Get Paginated Files Metadata";

    private Mock<ILogger<GetPaginatedFilesMetadata>> _mockLogger = null!;

    private Mock<IFileRepository> _mockRepository = null!;

    private Mock<IFileLocationService> _mockLocationService = null!;

    private Mock<IIdEncoderService<int>> _mockIdEncoderService = null!;

    private GetPaginatedFilesMetadata _query;

    private int _requestPageNumber;

    private int _requestPageSize;

    private CancellationToken _testCancellationToken;

    private List<StoredFile> _testStoredFiles;

    private string _testExternalId1;

    private string _testExternalId2;

    private string _testAccessLocation1;

    private string _testAccessLocation2;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<GetPaginatedFilesMetadata>>(MockBehavior.Strict);

        _mockRepository = new Mock<IFileRepository>(MockBehavior.Strict);

        _mockLocationService = new Mock<IFileLocationService>(MockBehavior.Strict);

        _mockIdEncoderService = new Mock<IIdEncoderService<int>>(MockBehavior.Strict);

        _query = new GetPaginatedFilesMetadata(_mockLogger.Object,
                                               _mockRepository.Object,
                                               _mockLocationService.Object,
                                               _mockIdEncoderService.Object);

        _requestPageNumber = 1;

        _requestPageSize = 10;

        _testCancellationToken = CancellationToken.None;

        _testStoredFiles =
        [
            new StoredFile
            {
                Id = 123,
                Name = "Test Name 1",
                StorageKey = "Test Storage Key 1",
                ContentType = "Test Content Type 1",
                CreatedAt = new DateTime(2020, 01, 05),
                Tags = [],
                UserId = 456
            },
            new StoredFile
            {
                Id = 789,
                Name = "Test Name 2",
                StorageKey = "Test Storage Key 2",
                ContentType = "Test Content Type 2",
                CreatedAt = new DateTime(2021, 12, 05),
                Tags = [],
                UserId = 101
            }
        ];

        _testExternalId1 = "Test External Id 1";

        _testExternalId2 = "Test External Id 2";

        _testAccessLocation1 = "Test Access Location 1";

        _testAccessLocation2 = "Test Access Location 2";
    }

    [TestCase(-1)]
    [TestCase(0)]
    public async Task ExecuteAsync_WhenPageNumberIsLessThanOne_ShouldReturnInvalid(int pageNumber)
    {
        // Arrange
        _requestPageNumber = pageNumber;

        _mockLogger.Setup(LogLevel.Error, $"Query '{Name}' failed validation: PageNumber:{Environment.NewLine}  - Must be greater than or equal to 1.");

        // Act
        var result = await Act();

        // Assert
        AssertInvalid(result);
    }

    [TestCase(0)]
    [TestCase(101)]
    public async Task ExecuteAsync_WhenPageSizeIsOutOfRange_ShouldReturnInvalid(int pageSize)
    {
        // Arrange
        _requestPageSize = pageSize;

        _mockLogger.Setup(LogLevel.Error, $"Query '{Name}' failed validation: PageSize:{Environment.NewLine}  - Must be between 1 and 100, inclusive.");

        // Act
        var result = await Act();

        // Assert
        AssertInvalid(result);
    }

    [Test]
    public async Task ExecuteAsync_WhenRepositoryReturnsEmptyCollection_ShouldReturnSuccess()
    {
        // Arrange
        Setup_Logger_Starting();

        _testStoredFiles = [];

        Setup_Repository_GetPaginatedFilesAsync();

        Setup_Logger_Completed();

        // Act
        var result = await Act();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            AssertSuccess(result);

            Assert.That(result.Value, Is.Empty);
        }
    }

    [Test]
    public async Task ExecuteAsync_WhenRepositoryReturnsCollection_ShouldReturnSuccess()
    {
        // Arrange
        Setup_Logger_Starting();

        Setup_Repository_GetPaginatedFilesAsync();

        Setup_IdEncoderService_Encode();

        Setup_LocationService_GetFileAccessLocation();

        Setup_Logger_Completed();

        // Act
        var result = await Act();

        using (Assert.EnterMultipleScope())
        {
            AssertSuccess(result);

            Assert.That(result.Value, Has.Count.EqualTo(2));

            Assert.That(result.Value?[0].Id, Is.EqualTo(_testExternalId1));
            Assert.That(result.Value?[0].Name, Is.EqualTo(_testStoredFiles[0].Name));
            Assert.That(result.Value?[0].AccessLocation, Is.EqualTo(_testAccessLocation1));
            Assert.That(result.Value?[0].ContentType, Is.EqualTo(_testStoredFiles[0].ContentType));
            Assert.That(result.Value?[0].CreatedAt, Is.EqualTo(_testStoredFiles[0].CreatedAt));
            Assert.That(result.Value?[0].Tags, Is.EquivalentTo(_testStoredFiles[0].Tags));

            Assert.That(result.Value?[1].Id, Is.EqualTo(_testExternalId2));
            Assert.That(result.Value?[1].Name, Is.EqualTo(_testStoredFiles[1].Name));
            Assert.That(result.Value?[1].AccessLocation, Is.EqualTo(_testAccessLocation2));
            Assert.That(result.Value?[1].ContentType, Is.EqualTo(_testStoredFiles[1].ContentType));
            Assert.That(result.Value?[1].CreatedAt, Is.EqualTo(_testStoredFiles[1].CreatedAt));
            Assert.That(result.Value?[1].Tags, Is.EquivalentTo(_testStoredFiles[1].Tags));
        }
    }

    private async Task<Result<List<FileMetadata>>> Act() => await _query.ExecuteAsync(_requestPageNumber, _requestPageSize, _testCancellationToken);

    private void Setup_Logger_Starting()
    {
        _mockLogger.Setup(LogLevel.Information, $"Query '{Name}' started with page number '{_requestPageNumber}', page count '{_requestPageSize}'.");
    }

    private void Setup_Logger_Completed()
    {
        _mockLogger.Setup(LogLevel.Information, $"Query '{Name}' completed with page number '{_requestPageNumber}', page count '{_requestPageSize}'.");
    }

    private void Setup_Repository_GetPaginatedFilesAsync(int times = 1)
    {
        _mockRepository
            .Setup(x => x.GetPaginatedFilesAsync(
                It.Is<int>(y => y == _requestPageNumber - 1),
                It.Is<int>(y => y == _requestPageSize),
                It.Is<CancellationToken>(y => y == _testCancellationToken)))
            .ReturnsAsync(_testStoredFiles)
            .Verifiable(Times.Exactly(times));
    }

    private void Setup_IdEncoderService_Encode()
    {
        _mockIdEncoderService
            .Setup(x => x.Encode(
                It.Is<int>(y => y == _testStoredFiles[0].Id)))
            .Returns(_testExternalId1)
            .Verifiable(Times.Once);

        _mockIdEncoderService
            .Setup(x => x.Encode(
                It.Is<int>(y => y == _testStoredFiles[1].Id)))
            .Returns(_testExternalId2)
            .Verifiable(Times.Once);
    }

    private void Setup_LocationService_GetFileAccessLocation()
    {
        _mockLocationService
            .Setup(x => x.GetFileAccessLocation(
                It.Is<string>(y => y == _testExternalId1)))
            .Returns(_testAccessLocation1)
            .Verifiable(Times.Once);

        _mockLocationService
            .Setup(x => x.GetFileAccessLocation(
                It.Is<string>(y => y == _testExternalId2)))
            .Returns(_testAccessLocation2)
            .Verifiable(Times.Once);
    }
}

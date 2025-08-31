using FileHub.Application.Commands;
using FileHub.Application.Queries;
using FileHub.Core.Common;
using FileHub.Core.Files;
using FileHub.Web.Controllers;
using FileHub.Web.Models;
using FileHub.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace FileHub.Web.Tests.Controllers;

[TestFixture]
public class FileControllerTests : TestBase
{
    private Mock<ICurrentUserService> _mockCurrentUserService;

    private Mock<IAddFile> _mockAddFile;

    private Mock<IGetFileContent> _mockGetFile;

    private Mock<IGetFileMetadata> _mockGetFileMetadata;

    private Mock<IGetPaginatedFileMetadata> _mockGetPaginatedFileMetadata;

    private FileController _controller;

    private CancellationToken _testCancellationToken;

    private string _testFileId;

    private Stream _testFileStream;

    private FormFile _testFile;

    private string _testContentType;

    private string _testFileName;

    private string _testName;

    private List<string> _testTags;

    private string _testAccessLocation;

    private DateTime _testCreatedAt;

    private UploadFileRequest _uploadFileRequest;

    private int _testUserId;

    private Result _testAddFileResult;

    private FileContent _testFileContent;

    private Result<FileContent> _testGetFileContentResult;

    private FileMetadata _testFileMetadata;

    private Result<FileMetadata> _testGetFileMetadataResult;

    private int _testPage;

    private int _testCount;

    private List<FileMetadata> _testFileMetadataList;

    private Result<List<FileMetadata>> _testGetPaginatedFileMetadataResult;

    [SetUp]
    public void SetUp()
    {
        _mockCurrentUserService = new Mock<ICurrentUserService>(MockBehavior.Strict);
        _mockAddFile = new Mock<IAddFile>(MockBehavior.Strict);
        _mockGetFile = new Mock<IGetFileContent>(MockBehavior.Strict);
        _mockGetFileMetadata = new Mock<IGetFileMetadata>(MockBehavior.Strict);
        _mockGetPaginatedFileMetadata = new Mock<IGetPaginatedFileMetadata>(MockBehavior.Strict);

        _controller = new FileController(_mockCurrentUserService.Object,
                                         _mockAddFile.Object,
                                         _mockGetFile.Object,
                                         _mockGetFileMetadata.Object,
                                         _mockGetPaginatedFileMetadata.Object);

        _testCancellationToken = CancellationToken.None;

        _testFileId = "Test File ID";

        _testFileStream = new MemoryStream("Test Content"u8.ToArray());

        _testContentType = "Test Content Type";

        _testFileName = "Test File";

        _testFile = new FormFile(_testFileStream, 0, _testFileStream.Length, "Test File", _testFileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = _testContentType
        };

        _testName = "Test Name";

        _testTags = ["Test Tag 1", "Test Tag 2", "Test Tag 3"];

        _uploadFileRequest = new UploadFileRequest
        {
            File = _testFile,
            Name = _testName,
            Tags = _testTags,
        };

        _testUserId = 123;

        _testAddFileResult = Result.Success();

        _testFileContent = new FileContent()
        {
            ContentType = _testContentType,
            Content = _testFileStream
        };

        _testGetFileContentResult = Result<FileContent>.Success(_testFileContent);

        _testAccessLocation = "Test Access Location";

        _testCreatedAt = new DateTime(2025, 03, 02);

        _testFileMetadata = new FileMetadata
        {
            Id = _testFileId,
            Name = _testName,
            Tags = _testTags,
            AccessLocation = _testAccessLocation,
            CreatedAt = _testCreatedAt,
            ContentType = _testContentType
        };

        _testGetFileMetadataResult = Result<FileMetadata>.Success(_testFileMetadata);

        _testFileMetadataList =
        [
            new FileMetadata
            {
                Id = "Test File ID 1",
                Name = "Test File Name 1",
                AccessLocation = "Test Access Location 1",
                CreatedAt = new DateTime(2025, 12, 12),
                ContentType = "Test Content Type 1",
                Tags = ["Test Tag 1", "Test Tag 2", "Test Tag 3"]
            },
            new FileMetadata
            {
                Id = "Test File ID 2",
                Name = "Test File Name 2",
                AccessLocation = "Test Access Location 2",
                CreatedAt = new DateTime(2024, 9, 8),
                ContentType = "Test Content Type 2",
                Tags = ["Test Tag 4", "Test Tag 5"]
            }
        ];

        _testPage = 1;

        _testCount = 2;

        _testGetPaginatedFileMetadataResult = Result<List<FileMetadata>>.Success(_testFileMetadataList);
    }

    [Test]
    public async Task UploadFileAsync_WhenUserIdZero_ShouldReturnUnauthorized()
    {
        // Arrange
        _testUserId = 0;

        Setup_CurrentUserService_GetCurrentUserId();

        // Act
        var result = await _controller.UploadFileAsync(_uploadFileRequest, _testCancellationToken);

        // Assert
        Assert.That(result, Is.EqualTo(Results.Unauthorized()));
    }

    [Test]
    public async Task UploadFileAsync_WhenAddFileFails_ShouldReturnInternalServerError()
    {
        // Arrange
        Setup_CurrentUserService_GetCurrentUserId();

        _testAddFileResult = Result.Failed();

        Setup_AddFile_ExecuteAsync();

        // Act
        var result = await _controller.UploadFileAsync(_uploadFileRequest, _testCancellationToken);

        // Assert
        Assert.That(result, Is.EqualTo(Results.InternalServerError()));
    }

    [Test]
    public async Task UploadFileAsync_WhenAddFileSucceeds_ShouldReturnSuccess()
    {
        // Arrange
        Setup_CurrentUserService_GetCurrentUserId();

        Setup_AddFile_ExecuteAsync();

        // Act
        var result = await _controller.UploadFileAsync(_uploadFileRequest, _testCancellationToken);

        // Assert
        Assert.That(result, Is.EqualTo(Results.Ok()));
    }

    [Test]
    public async Task GetFileContentAsync_WhenGetFileFails_ShouldReturnInternalServerError()
    {
        // Arrange
        _testGetFileContentResult = Result<FileContent>.Failed();

        Setup_GetFileContent_ExecuteAsync();

        // Act
        var result = await _controller.GetFileContentAsync(_testFileId, _testCancellationToken);

        // Assert
        Assert.That(result, Is.EqualTo(Results.InternalServerError()));
    }

    [Test]
    public async Task GetFileContentAsync_WhenGetFileSucceeds_ShouldReturnFile()
    {
        // Arrange
        Setup_GetFileContent_ExecuteAsync();

        // Act
        var result = await _controller.GetFileContentAsync(_testFileId, _testCancellationToken);

        // Assert
        Assert.That(result, Is.TypeOf<FileStreamHttpResult>());

        var fileResult = result as FileStreamHttpResult;

        Assert.That(fileResult?.FileStream, Is.EqualTo(_testFileStream));
    }

    [Test]
    public async Task GetFileMetadataAsync_WhenGetFileFails_ShouldReturnInternalServerError()
    {
        // Arrange
        _testGetFileMetadataResult = Result<FileMetadata>.Failed();

        Setup_GetFileMetadata_ExecuteAsync();

        // Act
        var result = await _controller.GetFileMetadataAsync(_testFileId, _testCancellationToken);

        // Assert
        Assert.That(result, Is.EqualTo(Results.InternalServerError()));
    }

    [Test]
    public async Task GetFileMetadataAsync_WhenGetFileSucceeds_ShouldReturnFileMetadata()
    {
        // Arrange
        Setup_GetFileMetadata_ExecuteAsync();

        // Act
        var result = await _controller.GetFileMetadataAsync(_testFileId, _testCancellationToken);

        // Assert
        Assert.That(result, Is.EqualTo(Results.Ok(_testFileMetadata)).UsingPropertiesComparer());
    }

    [Test]
    public async Task GetPaginatedFilesMetadataAsync_WhenGetFilesFails_ShouldReturnInternalServerError()
    {
        // Arrange
        _testGetPaginatedFileMetadataResult = Result<List<FileMetadata>>.Failed();

        Setup_GetPaginatedFilesMetadata_ExecuteAsync();

        // Act
        var result = await _controller.GetPaginatedFileMetadataAsync(_testPage, _testCount, _testCancellationToken);

        // Assert
        Assert.That(result, Is.EqualTo(Results.InternalServerError()));
    }

    [Test]
    public async Task GetPaginatedFilesMetadataAsync_WhenGetFilesSucceeds_ShouldReturnOk()
    {
        // Arrange
        Setup_GetPaginatedFilesMetadata_ExecuteAsync();

        // Act
        var result = await _controller.GetPaginatedFileMetadataAsync(_testPage, _testCount, _testCancellationToken);

        // Assert
        Assert.That(result, Is.EqualTo(Results.Ok(_testFileMetadataList)).UsingPropertiesComparer());
    }

    private void Setup_CurrentUserService_GetCurrentUserId(int times = 1)
    {
        _mockCurrentUserService
            .Setup(x => x.GetCurrentUserId())
            .Returns(_testUserId)
            .Verifiable(Times.Exactly(times));
    }

    private void Setup_AddFile_ExecuteAsync(int times = 1)
    {
        _mockAddFile
            .Setup(x => x.ExecuteAsync(
                It.IsAny<Stream>(),
                It.Is<string>(y => y == _testContentType),
                It.Is<string>(y => y == _testFileName),
                It.Is<string>(y => y == _testName),
                It.Is<int>(y => y == _testUserId),
                It.Is<List<string>>(y => y == _testTags),
                It.Is<CancellationToken>(y => y == _testCancellationToken)))
            .ReturnsAsync(_testAddFileResult)
            .Verifiable(Times.Exactly(times));
    }

    private void Setup_GetFileContent_ExecuteAsync(int times = 1)
    {
        _mockGetFile
            .Setup(x => x.ExecuteAsync(
                It.Is<string>(y => y == _testFileId),
                It.Is<CancellationToken>(y => y == _testCancellationToken)))
            .ReturnsAsync(_testGetFileContentResult)
            .Verifiable(Times.Exactly(times));
    }

    private void Setup_GetFileMetadata_ExecuteAsync(int times = 1)
    {
        _mockGetFileMetadata
            .Setup(x => x.ExecuteAsync(
                It.Is<string>(y => y == _testFileId),
                It.Is<CancellationToken>(y => y == _testCancellationToken)))
            .ReturnsAsync(_testGetFileMetadataResult)
            .Verifiable(Times.Exactly(times));
    }

    private void Setup_GetPaginatedFilesMetadata_ExecuteAsync(int times = 1)
    {
        _mockGetPaginatedFileMetadata
            .Setup(x => x.ExecuteAsync(
                It.Is<int>(y => y == _testPage),
                It.Is<int>(y => y == _testCount),
                It.Is<CancellationToken>(y => y == _testCancellationToken)))
            .ReturnsAsync(_testGetPaginatedFileMetadataResult)
            .Verifiable(Times.Exactly(times));

    }
}

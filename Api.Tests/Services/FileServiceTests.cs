using Api.Data.Entities;
using Api.Factories;
using Api.Models.Common;
using Api.Models.Files;
using Api.Repositories;
using Api.Services;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Api.Tests.Services;

public class FileServiceTests
{
    private readonly Mock<IStorageService> _storageService;
    private readonly Mock<ITagService> _tagService;
    private readonly Mock<IFileRepository> _fileRepository;
    private readonly FileService _fileService;

    public FileServiceTests()
    {
        _storageService = new Mock<IStorageService>();
        _tagService = new Mock<ITagService>();
        _fileRepository = new Mock<IFileRepository>();
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext
        {
            Request = { Scheme = "https", Host = new HostString("localhost") },
        };
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        var responseFactory = new ResponseFactory(httpContextAccessor.Object);
        _fileService = new FileService(
            _storageService.Object,
            _tagService.Object,
            _fileRepository.Object,
            responseFactory,
            NullLogger<FileService>.Instance
        );
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AddFileAsync_WhenFileIsValidOrNot_ReturnsCorrespondingResult(bool isValid)
    {
        // Arrange
        var file = new Mock<IFormFile>();
        var user = new User { Id = 1, UserName = "testUser" };
        var request = new UploadRequest { File = file.Object, Name = "test.txt", Tags = ["tag1", "tag2"] };

        _storageService
            .Setup(x => x.IsValidFile(file.Object))
            .Returns(isValid);
        
        _storageService
            .Setup(x => x.CreateFilePath(file.Object))
            .Returns("path/test.txt"); 
        
        // Act
        var result = await _fileService.AddFileAsync(request, user);
        
        // Assert
        result.IsSuccess.Should().Be(isValid);
        
        if (isValid)
            _fileRepository.Verify(x => x.AddAsync(It.IsAny<StoredFile>()), Times.Once);
        else
            result.Error.Should().Be(Error.NoFileUploaded);
    }

    [Fact]
    public async Task AddFileAsync_WhenSavingToDiskFails_ReturnsFailureResult()
    {
        // Arrange
        var file = new Mock<IFormFile>();
        var user = new User { Id = 1, UserName = "testUser" };
        var request = new UploadRequest { File = file.Object, Name = "test.txt", Tags = ["tag1", "tag2"] };
        
        _storageService
            .Setup(x => x.IsValidFile(file.Object))
            .Returns(true);
        
        _storageService
            .Setup(x => x.SaveFileToPathAsync(file.Object, It.IsAny<string>()))
            .Throws(new Exception());
        
        // Act
        var result = await _fileService.AddFileAsync(request, user);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Error.FileStorageError);
    }

    [Fact]
    public async Task GetFileContentByIdAsync_WithInvalidId_ReturnsNone()
    {
        // Arrange & Act
        var contentResponse = await _fileService.GetFileContentByIdAsync(-1);
        
        // Assert
        contentResponse.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task GetFileContentByIdAsync_WithNonExistingId_ReturnsNone()
    {
        // Arrange
        const int fileId = 1;
        _fileRepository
            .Setup(x => x.GetByIdAsync(fileId))
            .ReturnsAsync((StoredFile)null!);
        
        // Act
        var contentResponse = await _fileService.GetFileContentByIdAsync(fileId);
        
        // Assert
        contentResponse.HasValue.Should().BeFalse();
    }
    
    [Fact]
    public async Task GetFileContentByIdAsync_WithValidId_ReturnsFileContent()
    {
        // Arrange
        var content = new byte[] { 1, 2, 3 };
        var storedFile = new StoredFile
        {
            Id = 1,
            Path = "test.txt",
            ContentType = "text/plain",
            Name = "name",
            Uploader = new User { Id = 1, UserName = "testUser" },
        };

        _fileRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(storedFile);

        // Create test file
        await File.WriteAllBytesAsync(storedFile.Path, content);

        // Act
        var contentResponse = await _fileService.GetFileContentByIdAsync(1);

        // Assert
        contentResponse.HasValue.Should().BeTrue();
        contentResponse.Value.ContentType.Should().Be(storedFile.ContentType);
        contentResponse.Value.Data.Should().BeEquivalentTo(content);

        // Cleanup
        File.Delete(storedFile.Path);
    }

    [Fact]
    public async Task GetFileMetaDataByIdAsync_WithInvalidId_ReturnsNone()
    {
        // Act
        var metaDataResponse = await _fileService.GetFileMetaDataByIdAsync(-1);

        // Assert
        metaDataResponse.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task GetFileMetaDataByIdAsync_WithNonExistingId_ReturnsNone()
    {
        // Arrange
        const int fileId = 1;
        _fileRepository
            .Setup(x => x.GetByIdAsync(fileId))
            .ReturnsAsync((StoredFile)null!);
        
        // Act
        var metadataResponse = await _fileService.GetFileMetaDataByIdAsync(fileId);
        
        // Assert
        metadataResponse.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task GetFileMetaDataByIdAsync_WithValidId_ReturnsFileMetaData()
    {
        // Arrange
        var file = new StoredFile
        {
            Id = 1,
            Name = "test.txt",
            ContentType = "text/plain",
            Uploader = new User { UserName = "testUser" },
            CreatedAt = DateTime.UtcNow,
            Path = "path/to/file",
        };
        
        _fileRepository
            .Setup(x => x.GetByIdAsync(file.Id))
            .ReturnsAsync(file);
        
        // Act
        var metadataResponse = await _fileService.GetFileMetaDataByIdAsync(file.Id);
        
        // Assert
        metadataResponse.HasValue.Should().BeTrue();
        metadataResponse.Value.Name.Should().Be(file.Name);
        metadataResponse.Value.ContentType.Should().Be(file.ContentType);
        metadataResponse.Value.Uploader.Should().Be(file.Uploader.UserName);
    }

    [Fact]
    public async Task GetAllFilesByTagAsync_WithValidTag_ReturnsFiles()
    {
        // Arrange
        var files = new List<StoredFile>
        {
            new()
            {
                Id = 1,
                Name = "test.txt",
                ContentType = "text/plain",
                Uploader = new User { UserName = "testUser" },
                CreatedAt = DateTime.UtcNow,
                Path = "path/to/file",
            },
        };

        _fileRepository
            .Setup(x => x.GetAllByTagAsync("test"))
            .ReturnsAsync(files);
        
        _tagService
            .Setup(x => x.TagExistsByNameAsync("test"))
            .ReturnsAsync(true);

        // Act
        var result = await _fileService.GetAllFilesByTagAsync("test");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var content = result.Content;
        content!.Value.Should().HaveCount(1);
        content.Value.First().Url.Should().StartWith("https://localhost/files/");
    }

    [Fact]
    public async Task GetAllFilesByTag_WithEmptyTag_ReturnsFailure()
    {
        // Act
        var result = await _fileService.GetAllFilesByTagAsync("");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Error.InvalidTagName);
    }

    [Fact]
    public async Task GetAllFilesByTagAsync_WithNonExistingTag_ReturnsFailure()
    {
        // Arrange
        const string tagName = "test";
        _tagService
            .Setup(x => x.TagExistsByNameAsync(tagName))
            .ReturnsAsync(false);
        
        // Act
        var result = await _fileService.GetAllFilesByTagAsync(tagName);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Error.TagNotFound);
    }

    [Fact]
    public async Task GetAllFilesByTagAsync_WithValidTagWithNoCorrespondingFiles_ReturnsSuccessWithNone()
    {
        // Arrange
        const string tagName = "test";
        
        _tagService
            .Setup(x => x.TagExistsByNameAsync(tagName))
            .ReturnsAsync(true);
        
        _fileRepository
            .Setup(x => x.GetAllByTagAsync(tagName))
            .ReturnsAsync([]);
        
        // Act
        var result = await _fileService.GetAllFilesByTagAsync(tagName);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Content!.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task GetPaginatedFilesAsync_WithValidPageAndPageSize_ReturnsFiles()
    {
        // Arrange
        const int page = 1;
        const int pageSize = 2;
        var files = new List<StoredFile>
        {
            new()
            {
                Id = 1,
                Name = "test.txt",
                ContentType = "text/plain",
                Uploader = new User { UserName = "testUser" },
                CreatedAt = DateTime.UtcNow,
                Path = "path/to/file1",
            },
            new()
            {
                Id = 2,
                Name = "test.txt",
                ContentType = "text/plain",
                Uploader = new User { UserName = "testUser" },
                CreatedAt = DateTime.UtcNow,
                Path = "path/to/file2",
            },
        };
        
        _fileRepository
            .Setup(x => x.GetPaginatedFilesAsync(page, pageSize))
            .ReturnsAsync(files);
        
        // Act
        var result = await _fileService.GetPaginatedFilesAsync(page, pageSize);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Content.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPaginatedFilesAsync_WithInvalidPage_ReturnsFailure()
    {
        // Arrange
        const int page = -1;
        const int pageSize = 2;

        // Act
        var result = await _fileService.GetPaginatedFilesAsync(page, pageSize);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Error.InvalidPage);
    }
}

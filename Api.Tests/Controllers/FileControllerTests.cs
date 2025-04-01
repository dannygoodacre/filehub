using System.Security.Claims;
using Api.Controllers;
using Api.Data.Entities;
using Api.Models.Common;
using Api.Models.Files;
using Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Api.Tests.Controllers;

public class FileControllerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly FileController _controller;

    private const string UserName = "testuser";
    private readonly User _user = new() { UserName = UserName };

    public FileControllerTests()
    {
        _userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!
        );

        _fileServiceMock = new Mock<IFileService>();
        _controller = new FileController(_userManagerMock.Object, _fileServiceMock.Object, NullLogger<FileController>.Instance);

        // Setup default authenticated user context
        var claims = new List<Claim> { new(ClaimTypes.Name, UserName) };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal },
        };
    }

    [Theory]
    [InlineData(null, 200, Messages.FileUploaded)]
    [InlineData(Error.NoFileUploaded, 400, Messages.NoFileUploaded)]
    [InlineData(Error.FileStorageError, 500, Messages.InternalServerError)]
    public async Task UploadFileAsync_ReturnsExpectedResult(Error? internalError, int expectedStatusCode, string? expectedMessage)
    {
        // Arrange
        var file = new Mock<IFormFile>();
        file.Setup(f => f.FileName).Returns("test");
        var request = new UploadRequest { File = file.Object, Name = null! };
        
        _userManagerMock.Setup(x => x.FindByNameAsync(UserName)).ReturnsAsync(_user);
        _fileServiceMock
            .Setup(x => x.AddFileAsync(request, _user))
            .ReturnsAsync(internalError is null ? Result.Success() : Result.Failure(internalError.Value));
        
        // Act
        var result = await _controller.UploadFileAsync(request);
        
        // Assert
        result
            .Should().BeAssignableTo<IStatusCodeActionResult>()
            .Which.StatusCode.Should().Be(expectedStatusCode);
        
        result
            .Should().BeAssignableTo<ObjectResult>()
            .Which.Value.Should().Be(expectedMessage);
    }

    [Fact]
    public async Task UploadFileAsync_WhenUserNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();
        var request = new UploadRequest { File = null!, Name = null! };

        // Act
        var result = await _controller.UploadFileAsync(request);

        // Assert
        result
            .Should().BeAssignableTo<IStatusCodeActionResult>()
            .Which.StatusCode.Should().Be(401);
        
        result
            .Should().BeAssignableTo<ObjectResult>()
            .Which.Value.Should().Be(Messages.NotLoggedIn);
    }

    [Fact]
    public async Task GetFileAsync_WhenFileExists_ReturnsFileResult()
    {
        // Arrange
        const int fileId = 1;
        var fileContent = new ContentResponse()
        {
            Data = [1, 2, 3],
            ContentType = "application/pdf",
        };

        _userManagerMock.Setup(x => x.FindByNameAsync(UserName)).ReturnsAsync(_user);
        _fileServiceMock.Setup(x => x.GetFileContentByIdAsync(fileId)).ReturnsAsync(Maybe.Some(fileContent));

        // Act
        var result = await _controller.GetFileAsync(fileId);

        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.FileContents.Should().BeEquivalentTo(fileContent.Data);
        fileResult.ContentType.Should().Be(fileContent.ContentType);
    }

    [Fact]
    public async Task GetFileAsync_WhenFileNotFound_ReturnsNotFound()
    {
        // Arrange
        const int fileId = 1;
        _userManagerMock.Setup(x => x.FindByNameAsync(UserName)).ReturnsAsync(_user);
        _fileServiceMock
            .Setup(x => x.GetFileContentByIdAsync(fileId))
            .ReturnsAsync(Maybe<ContentResponse>.None());

        // Act
        var result = await _controller.GetFileAsync(fileId);

        // Assert
        result
            .Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().Be(Messages.FileNotFound);
    }

    [Fact]
    public async Task GetFileMetadataAsync_WhenFileExists_ReturnsFileMetadata()
    {
        // Arrange
        const int fileId = 1;
        var fileMetadata = new MetaDataResponse()
        {
            Name = "test.pdf",
            Tags = ["test"],
            Url = "url/to/file",
            ContentType = "application/pdf",
            CreatedAt = default,
            Uploader = UserName,
        };
        
        _userManagerMock.Setup(x => x.FindByNameAsync(UserName)).ReturnsAsync(_user);
        _fileServiceMock
            .Setup(x => x.GetFileMetaDataByIdAsync(fileId))
            .ReturnsAsync(Maybe.Some(fileMetadata));
 
        // Act
        var result = await _controller.GetFileMetadataAsync(fileId);
        
        // Assert
        result
            .Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(fileMetadata);
    }

    [Fact]
    public async Task GetFileMetadataAsync_WhenFileDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        const int fileId = 1;
        _userManagerMock.Setup(x => x.FindByNameAsync(UserName)).ReturnsAsync(_user);
        _fileServiceMock
            .Setup(x => x.GetFileMetaDataByIdAsync(fileId))
            .ReturnsAsync(Maybe<MetaDataResponse>.None());
        
        // Act
        var result = await _controller.GetFileMetadataAsync(fileId);
        
        // Assert
        result
            .Should().BeOfType<NotFoundObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetAllFilesByTagAsync_WhenTagExists_ReturnsFiles()
    {
        // Arrange
        const string tagName = "test";
        var files = new List<MetaDataResponse>
        {
            new()
            {
                Name = "test.pdf",
                Tags = ["test"],
                Url = "url/to/file",
                ContentType = "application/pdf",
                CreatedAt = default,
                Uploader = UserName,
            },
        };

        _userManagerMock.Setup(x => x.FindByNameAsync(UserName)).ReturnsAsync(_user);
        _fileServiceMock
            .Setup(x => x.GetAllFilesByTagAsync(tagName))
            .ReturnsAsync(Result.Success(Maybe.Some(files)));

        // Act
        var result = await _controller.GetAllFilesByTagAsync(tagName);

        // Assert
        result
            .Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(files);
    }
    
    [Fact]
    public async Task GetAllFilesByTagAsync_WhenNoFilesHaveGivenTag_ReturnsOk()
    {
        // Arrange
        const string tagName = "test";
        _userManagerMock.Setup(x => x.FindByNameAsync(UserName)).ReturnsAsync(_user);
        _fileServiceMock
            .Setup(x => x.GetAllFilesByTagAsync(tagName))
            .ReturnsAsync(Result.Success(Maybe<List<MetaDataResponse>>.None()));
        
        // Act
        var result = await _controller.GetAllFilesByTagAsync(tagName);
        
        // Assert
        result
            .Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new List<MetaDataResponse>());
    }

    [Theory]
    [InlineData(Error.InvalidTagName, 400, Messages.InvalidTagName)]
    [InlineData(Error.TagNotFound, 404, Messages.TagNotFound)]
    public async Task GetAllFilesByTagAsync_WhenInternalErrorOccurs_ReturnsExpectedMessages(
        Error internalError, int expectedStatusCode, string expectedMessage)
    {
        // Arrange
        const string tagName = "test";
        _userManagerMock.Setup(x => x.FindByNameAsync(UserName)).ReturnsAsync(_user);
        _fileServiceMock
            .Setup(x => x.GetAllFilesByTagAsync(tagName))
            .ReturnsAsync(Result<Maybe<List<MetaDataResponse>>>.Failure(internalError));

        // Act
        var result = await _controller.GetAllFilesByTagAsync(tagName);

        // Assert
        result.Should().BeAssignableTo<IStatusCodeActionResult>()    
            .Which.StatusCode.Should().Be(expectedStatusCode);
        
        result.Should().BeAssignableTo<ObjectResult>()
            .Which.Value.Should().BeEquivalentTo(expectedMessage);
    }

    [Fact]
    public async Task GetPaginatedFilesAsync_WhenFilesExist_ReturnsFiles()
    {
        // Arrange
        const int page = 1;
        const int pageSize = 3;
        var files = new List<MetaDataResponse>
        {
            new()
            {
                Name = "test.pdf",
                Tags = ["test"],
                Url = "url/to/file",
                ContentType = "application/pdf",
                CreatedAt = default,
                Uploader = UserName,
            },
            new()
            {
                Name = "test.pdf",
                Tags = ["test"],
                Url = "url/to/file",
                ContentType = "application/pdf",
                CreatedAt = default,
                Uploader = UserName,
            },
            new()
            {
                Name = "test.pdf",
                Tags = ["test"],
                Url = "url/to/file",
                ContentType = "application/pdf",
                CreatedAt = default,
                Uploader = UserName,
            },
        };
        
        _userManagerMock.Setup(x => x.FindByNameAsync(UserName)).ReturnsAsync(_user);
        _fileServiceMock
            .Setup(x => x.GetPaginatedFilesAsync(page, pageSize))
            .ReturnsAsync(Result.Success(files));
        
        // Act
        var result = await _controller.GetPaginatedFilesAsync(page, pageSize);
        
        // Assert
        result
            .Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(files);
    }

    [Fact]
    public async Task GetPaginatedFilesAsync_WhenRequestedPageIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        const int page = -1;
        const int pageSize = 3;
        
        _userManagerMock.Setup(x => x.FindByNameAsync(UserName)).ReturnsAsync(_user);
        _fileServiceMock
            .Setup(x => x.GetPaginatedFilesAsync(page, pageSize))
            .ReturnsAsync(Result<List<MetaDataResponse>>.Failure(Error.InvalidPage));
        
        // Act
        var result = await _controller.GetPaginatedFilesAsync(page, pageSize);
        
        // Assert
        result.Should().BeAssignableTo<IStatusCodeActionResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        
        result.Should().BeAssignableTo<ObjectResult>()
            .Which.Value.Should().BeEquivalentTo(Messages.InvalidPageRequested);
    }
}

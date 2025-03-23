using Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Api.Tests.Services;

public class StorageServiceTests
{
    private readonly StorageService _sut;
    private const string TestFileDirectory = "/TestFiles";

    public StorageServiceTests()
    {
        Mock<IConfiguration> configurationMock = new();
        configurationMock.Setup(x => x["FileDirectory"]).Returns(TestFileDirectory);
        _sut = new StorageService(configurationMock.Object);
    }

    [Fact]
    public void GetFileDirectory_ShouldReturnConfiguredPath()
    {
        // Act
        var result = _sut.GetFileDirectory();

        // Assert
        result.Should().Be(TestFileDirectory);
    }

    [Fact]
    public void CreateFileName_ShouldCreateValidFileName()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.jpg");

        // Act
        var result = _sut.CreateFileName(fileMock.Object);

        // Assert
        result.Should().Match("????????????_*-*-*-*-*.jpg");
    }

    [Theory]
    [InlineData(true, 1024)]
    [InlineData(false, 0)]
    public void IsValidFile_ShouldValidateFileCorrectly(bool expected, long fileLength)
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(fileLength);

        // Act
        var result = _sut.IsValidFile(fileMock.Object);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IsValidFile_WithNullFile_ShouldReturnFalse()
    {
        // Act
        var result = _sut.IsValidFile(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CreateFilePath_ShouldCombineDirectoryAndFileName()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.jpg");

        // Act
        var result = _sut.CreateFilePath(fileMock.Object);

        // Assert
        result.Should().StartWith(TestFileDirectory);
        result.Should().EndWith(".jpg");
    }

    [Fact]
    public async Task SaveFileToPathAsync_ShouldSaveFile()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var testPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act & Assert
        await _sut.Invoking(async x => await x.SaveFileToPathAsync(fileMock.Object, testPath))
            .Should().NotThrowAsync();
    }
}

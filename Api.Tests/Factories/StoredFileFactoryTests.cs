using Api.Data.Entities;
using Api.Factories;

namespace Api.Tests.Factories;

public class StoredFileFactoryTests
{
    private const string FileName = "test.txt";
    private const string FilePath = "/files/test.txt";
    private const string ContentType = "text/plain";
    private const string UserName = "testuser";

    private readonly User _user = new() { UserName = UserName };
    private readonly List<Tag> _tags = [new() { Name = "test" }];

    [Fact]
    public void Create_WithValidInputs_ReturnsStoredFileWithCorrectProperties()
    {
        // Act
        var result = StoredFileFactory.Create(
            FileName,
            FilePath,
            ContentType,
            _user,
            _tags
        );

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(FileName);
        result.Path.Should().Be(FilePath);
        result.ContentType.Should().Be(ContentType);
        result.Uploader.Should().Be(_user);
        result.Tags.Should().BeEquivalentTo(_tags);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithNullTags_ReturnsStoredFileWithNullTags()
    {
        // Act
        var result = StoredFileFactory.Create(
            FileName,
            FilePath,
            ContentType,
            _user,
            null
        );

        // Assert
        result.Should().NotBeNull();
        result.Tags.Should().BeNull();
    }

    [Fact]
    public void Create_SetsCreatedAtToCurrentUtcTime()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow;

        // Act
        var result = StoredFileFactory.Create(
            "test.txt",
            "/files/test.txt",
            "text/plain",
            _user,
            null
        );
        var afterCreate = DateTime.UtcNow;

        // Assert
        result.CreatedAt.Should().BeOnOrAfter(beforeCreate);
        result.CreatedAt.Should().BeOnOrBefore(afterCreate);
    }
}

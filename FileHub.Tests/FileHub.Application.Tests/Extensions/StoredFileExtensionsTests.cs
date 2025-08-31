using FileHub.Application.Extensions;
using FileHub.Core.Entities;

namespace FileHub.Application.Tests.Extensions;

[TestFixture]
public class StoredFileExtensionsTests : TestBase
{
    [Test]
    public void ToMetadata()
    {
        // Arrange
        var file = new StoredFile
        {
            Id = 123,
            Name = "Test Name",
            StorageKey = "Test Storage Key",
            Tags = [new Tag { Name = "Test Tag 1" }, new Tag { Name = "Test Tag 2" }],
            CreatedAt = DateTime.UtcNow,
            ContentType = "Test Content Type",
            UserId = 0,
        };

        const string externalId = "Test External Id";

        const string accessLocation = "Test Access Location";

        // Act
        var result = file.ToMetadata(externalId, accessLocation);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.EqualTo(externalId));
            Assert.That(result.Name, Is.EqualTo(file.Name));
            Assert.That(result.AccessLocation, Is.EqualTo(accessLocation));
            Assert.That(result.ContentType, Is.EqualTo(file.ContentType));
            Assert.That(result.CreatedAt, Is.EqualTo(file.CreatedAt));

            Assert.That(result.Tags, Has.Count.EqualTo(file.Tags.Count));
            Assert.That(result.Tags[0], Is.EqualTo(file.Tags.First().Name));
            Assert.That(result.Tags[1], Is.EqualTo(file.Tags.Skip(1).First().Name));
        }
    }
}

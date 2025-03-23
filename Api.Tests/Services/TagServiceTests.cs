using Api.Data.Entities;
using Api.Repositories;
using Api.Services;
using Moq;

namespace Api.Tests.Services;

public class TagServiceTests
{
    private readonly Mock<ITagRepository> _tagRepository;
    private readonly TagService _tagService;

    public TagServiceTests()
    {
        _tagRepository = new Mock<ITagRepository>();
        _tagService = new TagService(_tagRepository.Object);
    }

    [Fact]
    public async Task GetOrCreateTagsByNameAsync_OnlyExistingTags_ReturnsExistingTags()
    {
        // Arrange
        var tagNames = new List<string> { "existing1", "existing2" };
        var existingTags = tagNames.Select(name => new Tag { Name = name }).ToList();

        _tagRepository
            .Setup(r => r.GetTagsByNamesAsync(tagNames))
            .ReturnsAsync(existingTags);

        // Act
        var result = await _tagService.GetOrCreateTagsByNameAsync(tagNames);

        // Assert
        result.Should().BeEquivalentTo(existingTags);
        _tagRepository.Verify(r => r.AddRangeAsync(It.IsAny<List<Tag>>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateTagsByNameAsync_OnlyNewTags_CreatesAndReturnsNewTags()
    {
        // Arrange
        var tagNames = new List<string> { "new1", "new2" };
        _tagRepository
            .Setup(r => r.GetTagsByNamesAsync(tagNames))
            .ReturnsAsync(new List<Tag>());

        // Act
        var result = (await _tagService.GetOrCreateTagsByNameAsync(tagNames)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Select(t => t.Name).Should().BeEquivalentTo(tagNames);
        _tagRepository.Verify(r => r.AddRangeAsync(It.Is<List<Tag>>(tags =>
                        tags.Count == 2 && tags.All(t => tagNames.Contains(t.Name))
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetOrCreateTagsByNameAsync_MixedTags_ReturnsAllTags()
    {
        // Arrange
        const string existingTagName = "existing";
        const string newTagName = "new";
        var tagNames = new List<string> { existingTagName, newTagName };
        var existingTag = new Tag { Name = existingTagName };

        _tagRepository
            .Setup(r => r.GetTagsByNamesAsync(tagNames))
            .ReturnsAsync(new List<Tag> { existingTag });

        // Act
        var result = (await _tagService.GetOrCreateTagsByNameAsync(tagNames)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Select(t => t.Name).Should().BeEquivalentTo(tagNames);
        _tagRepository.Verify(r => r.AddRangeAsync(
                    It.Is<List<Tag>>(tags =>
                        tags.Count == 1 && tags[0].Name == newTagName
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetOrCreateTagsByNameAsync_EmptyTagNames_ReturnsEmptyCollection()
    {
        // Arrange
        var tagNames = new List<string>();

        // Act
        var result = await _tagService.GetOrCreateTagsByNameAsync(tagNames);

        // Assert
        result.Should().BeEmpty();
        _tagRepository.Verify(r => r.AddRangeAsync(It.IsAny<List<Tag>>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateTagsByNameAsync_DuplicateTagNames_HandlesDuplicatesCorrectly()
    {
        // Arrange
        var tagNames = new List<string> { "tag1", "tag1", "tag2" };
        var existingTag = new Tag { Name = "tag1" };

        _tagRepository
            .Setup(r => r.GetTagsByNamesAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<Tag> { existingTag });

        // Act
        var result = (await _tagService.GetOrCreateTagsByNameAsync(tagNames)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Select(t => t.Name).Should().BeEquivalentTo("tag1", "tag2");
        _tagRepository.Verify(r => r.AddRangeAsync(
                It.Is<List<Tag>>(tags => tags.Count == 1 && tags[0].Name == "tag2")
                ),
            Times.Once
        );
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TagExistsByNameAsync_WhenTagExistsOrNot_ReturnsExpectedResult(bool tagExists)
    {
        // Arrange
        const string tagName = "tagName";
        _tagRepository
            .Setup(r => r.TagExistsByNameAsync(tagName))
            .ReturnsAsync(tagExists);
        
        // Act
        var result = await _tagService.TagExistsByNameAsync(tagName);
        
        // Assert
        result.Should().Be(tagExists);
    }
}

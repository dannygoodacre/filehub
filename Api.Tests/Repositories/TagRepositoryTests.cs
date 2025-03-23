using Api.Data;
using Api.Data.Entities;
using Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Api.Tests.Repositories;

public class TagRepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly TagRepository _repository;

    private readonly List<Tag> _tags =
    [
        new() { Name = "Tag1" },
        new() { Name = "Tag2" },
        new() { Name = "Tag3" },
    ];

    public TagRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _repository = new TagRepository(_context);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddTagsSuccessfully()
    {
        // Act
        await _repository.AddRangeAsync(_tags);

        // Assert
        var savedTags = await _context.Tags.ToListAsync();
        savedTags.Should().HaveCount(3);
        savedTags.Should().Contain(t => t.Name == "Tag1");
        savedTags.Should().Contain(t => t.Name == "Tag2");
        savedTags.Should().Contain(t => t.Name == "Tag3");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllTags()
    {
        // Arrange
        await _context.Tags.AddRangeAsync(_tags);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(t => t.Name == "Tag1");
        result.Should().Contain(t => t.Name == "Tag2");
        result.Should().Contain(t => t.Name == "Tag3");
    }

    [Fact]
    public async Task GetTagsByNamesAsync_ShouldReturnMatchingTags()
    {
        // Arrange
        await _context.Tags.AddRangeAsync(_tags);
        await _context.SaveChangesAsync();

        var tagNames = new[] { "Tag1", "Tag3" };

        // Act
        var result = (await _repository.GetTagsByNamesAsync(tagNames)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Name == "Tag1");
        result.Should().Contain(t => t.Name == "Tag3");
    }

    [Fact]
    public async Task GetTagsByNamesAsync_WithEmptyNames_ShouldReturnEmptyCollection()
    {
        // Arrange
        await _context.Tags.AddRangeAsync(_tags);
        await _context.SaveChangesAsync();

        var tagNames = Array.Empty<string>();

        // Act
        var result = await _repository.GetTagsByNamesAsync(tagNames);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task TagExistsByNameAsync_ShouldReturnTrueForExistingTag()
    {
        // Arrange
        await _context.Tags.AddRangeAsync(_tags);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _repository.TagExistsByNameAsync("Tag1");
        
        // Assert
        result.Should().BeTrue();
    }
}

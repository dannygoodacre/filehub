using FileHub.Core.Entities;
using FileHub.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FileHub.Data.Tests.Repositories;

[TestFixture]
public class TagRepositoryTests : TestBase
{
    private ApplicationContext _context = null!;

    private TagRepository _repository = null!;

    [SetUp]
    public async Task SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationContext(options);

        _context.Tags.AddRange(new List<Tag>
        {
            new() { Name = "test tag 1" },
            new() { Name = "test tag 2" },
            new() { Name = "test tag 3" },
        });

        await _context.SaveChangesAsync();

        _repository = new TagRepository(_context);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _context.DisposeAsync();
    }

    [Test]
    public async Task AddRange_WhenEmptyCollection_ShouldAddNothing()
    {
        // Act
        _repository.AddRange([]);

        await _context.SaveChangesAsync();

        // Assert
        Assert.That(await _context.Tags.CountAsync(), Is.EqualTo(3));
    }

    [Test]
    public async Task AddRange_WhenCollection_ShouldAddNewTags()
    {
        // Arrange
        var tags = new List<Tag>
        {
            new() { Name = "test tag 4" },
            new() { Name = "test tag 5" },
        };

        // Act
        _repository.AddRange(tags);

        await _context.SaveChangesAsync();

        // Assert
        Assert.That(await _context.Tags.CountAsync(), Is.EqualTo(5));
    }

    [Test]
    public async Task GetManyAsync_WhenAllExist_ShouldReturnAllMatchingTags()
    {
        // Arrange
        var tagNames = new List<string> { "test tag 1", "test tag 2" };

        // Act
        var result = await _repository.GetManyForUpdateAsync(tagNames);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.Select(x => x.Name), Is.EquivalentTo(tagNames));
    }

    [Test]
    public async Task GetManyAsync_WhenSomeExist_ShouldReturnOnlyMatchingTags()
    {
        // Arrange
        var tagNames = new List<string> { "test tag 1", "test tag 4" };

        // Act
        var result = await _repository.GetManyForUpdateAsync(tagNames);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.Select(x => x.Name), Is.EquivalentTo(new List<string> { "test tag 1" }));
    }

    [Test]
    public async Task GetManyAsync_WhenNoneExist_ShouldReturnEmptyCollection()
    {
        // Arrange
        var tagNames = new List<string> { "test tag 4", "test tag 5" };

        // Act
        var result = await _repository.GetManyForUpdateAsync(tagNames);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [TestCase("test tag 1", true)]
    [TestCase("test tag 4", false)]
    public async Task ExistsAsync(string tagName, bool doesExist)
    {
        // Act
        var result = await _repository.ExistsAsync(tagName);

        // Assert
        Assert.That(result, Is.EqualTo(doesExist));
    }
}

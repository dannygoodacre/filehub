using FileHub.Core.Entities;
using FileHub.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FileHub.Data.Tests.Repositories;

[TestFixture]
public class CategoryRepositoryTests : TestBase
{
    private ApplicationContext _context = null!;

    private CategoryRepository _repository = null!;

    private readonly List<Category> _categories =
    [
        new()
        {
            Id = 123,
            Name = "Category 1"
        },
        new()
        {
            Id = 456,
            Name = "Category 2"
        }
    ];

    [SetUp]
    public async Task SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationContext(options);

        await _context.Categories.AddRangeAsync(_categories);

        await _context.SaveChangesAsync();

        _repository = new CategoryRepository(_context);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _context.DisposeAsync();
    }

    [Test]
    public async Task Add()
    {
        // Arrange
        var category = new Category
        {
            Id = 789,
            Name = "Category 3"
        };

        // Act
        _repository.Add(category);

        await _context.SaveChangesAsync();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(await _context.Categories.CountAsync(), Is.EqualTo(3));
            Assert.That(await _context.Categories.FirstAsync(x => x.Id == 789), Is.Not.Null);
        }
    }

    [Test]
    public async Task GetAllAsync_WhenFound_ShouldReturnCategories()
    {
        // Act
        var categories = await _repository.GetAllAsync();

        // Assert
        Assert.That(categories, Is.EquivalentTo(_categories));
    }

    [Test]
    public async Task GetAllAsync_WhenNoCategories_ShouldReturnEmpty()
    {
        // Arrange
        _context.Categories.RemoveRange(_context.Categories);

        await _context.SaveChangesAsync();

        // Act
        var categories = await _repository.GetAllAsync();

        // Assert
        Assert.That(categories, Is.Empty);
    }

    [Test]
    public async Task GetByNameAsync_WhenFound_ShouldReturnCategory()
    {
        // Arrange
        const string name = "Category 2";

        // Act
        var category = await _repository.GetByNameAsync(name);

        // Assert
        Assert.That(category, Is.EqualTo(_categories[1]).UsingPropertiesComparer());
    }

    [Test]
    public async Task GetByNameAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        const string name = "Non-existent Category";

        // Act
        var category = await _repository.GetByNameAsync(name);

        // Assert
        Assert.That(category, Is.Null);
    }

    [Test]
    public async Task GetByNameForUpdateAsync_WhenDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        const string name = "Non-existing category";

        // Act
        var category = await _repository.GetByNameForUpdateAsync(name);

        // Assert
        Assert.That(category, Is.Null);
    }

    [Test]
    public async Task GetByNameForUpdateAsync_WhenExists_ShouldReturnCategory()
    {
        // Arrange
        const string name = "Category 2";

        // Act
        var category = await _repository.GetByNameForUpdateAsync(name);

        // Assert
        Assert.That(category, Is.EqualTo(_categories[1]).UsingPropertiesComparer());
    }

    [TestCase("Category 1", true)]
    [TestCase("Non-existent category", false)]
    public async Task ExistsAsync_WhenDoesNotExist_ShouldReturnFalse(string name, bool exists)
    {
        // Act
        var result = await _repository.ExistsAsync(name);

        // Assert
        Assert.That(result, Is.EqualTo(exists));
    }
}

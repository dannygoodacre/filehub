using FileHub.Core.Entities;
using FileHub.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FileHub.Data.Tests.Repositories;

[TestFixture]
public class FileRepositoryTests : TestBase
{
    private ApplicationContext _context = null!;

    private FileRepository _repository = null!;

    private static readonly Category _category1 = new()
    {
        Id = 1,
        Name = "Category 1"
    };

    private static readonly Category _category2 = new()
    {
        Id = 2,
        Name = "Category 2"
    };

    private static readonly List<Tag> _tags =
    [
        new() { Id = 1, Name = "test tag 1" },
        new() { Id = 2, Name = "test tag 2" },
        new() { Id = 3, Name = "test tag 3" }
    ];

    private readonly List<StoredFile> _storedFiles =
    [
        new()
        {
            Id = 123,
            Name = "test file 1",
            ContentType = "test content type 1",
            StorageKey = "test storage key 1",
            CreatedAt = new DateTime(2025, 01, 01),
            Category = _category1,
            Tags = [_tags[0], _tags[1]],
            UserId = 0
        },

        new()
        {
            Id = 456,
            Name = "test file 2",
            ContentType = "test content type 2",
            StorageKey = "test storage key 2",
            CreatedAt = new DateTime(2025, 02, 01),
            Category = _category2,
            Tags = [_tags[2]],
            UserId = 0
        },

        new()
        {
            Id = 789,
            Name = "test file 3",
            ContentType = "test content type 3",
            StorageKey = "test storage key 3",
            CreatedAt = new DateTime(2025, 03, 01),
            Category = _category2,
            Tags = [_tags[1]],
            UserId = 0
        }
    ];

    [SetUp]
    public async Task SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationContext(options);

        _context.StoredFiles.AddRange(_storedFiles);

        await _context.SaveChangesAsync();

        _repository = new FileRepository(_context);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _context.DisposeAsync();
    }

    [Test]
    public async Task Add_WhenGivenFile_ShouldAddFile()
    {
        // Arrange
        var storedFile = new StoredFile()
        {
            Id = 101,
            Name = "test file 4",
            ContentType = "test content type 4",
            StorageKey = "test storage key 4",
            CreatedAt = new DateTime(2025, 04, 01),
            Category = _category1,
            Tags = [new Tag { Name = "test tag 1" }],
            UserId = 1
        };

        // Act
        _repository.Add(storedFile);

        await _context.SaveChangesAsync();

        // Assert
        using (Assert.EnterMultipleScope())
        {

            Assert.That(await _context.StoredFiles.CountAsync(), Is.EqualTo(4));
            Assert.That(await _context.StoredFiles.FirstAsync(file => file.Id == 101), Is.EqualTo(storedFile).UsingPropertiesComparer());
        }
    }

    [Test]
    public async Task GetByIdAsync_WhenFileDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        const int id = 100;

        // Act
        var result = await _repository.GetByIdAsync(id);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByIdAsync_WhenFileExists_ShouldReturnFile()
    {
        // Arrange
        const int id = 456;

        // Act
        var result = await _repository.GetByIdAsync(id);

        // Assert
        Assert.That(result, Is.EqualTo(_storedFiles[1]).UsingPropertiesComparer());
    }

    [Test]
    public async Task GetAllByTagAsync_WhenTagDoesNotExist_ShouldReturnEmptyCollection()
    {
        // Arrange
        const string tagName = "test tag 4";

        // Act
        var result = await _repository.GetAllByTagAsync(tagName);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetAllByTagAsync_WhenTagExists_ShouldReturnFiles()
    {
        // Arrange
        const string tagName = "test tag 2";

        // Act
        var result = await _repository.GetAllByTagAsync(tagName);

        Assert.That(result, Has.Count.EqualTo(2));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result[0].Id, Is.EqualTo(_storedFiles[0].Id));
            Assert.That(result[1].Id, Is.EqualTo(_storedFiles[2].Id));
        }
    }

    [Test]
    public async Task GetFilesCountAsync_WhenNoFiles_ShouldZero()
    {
        // Arrange
        _context.RemoveRange(_storedFiles);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetFilesCountAsync();

        // Assert
        Assert.That(result, Is.Zero);
    }

    [Test]
    public async Task GetFilesCountAsync_WhenFiles_ShouldReturnCount()
    {
        // Act
        var result = await _repository.GetFilesCountAsync();

        // Assert
        Assert.That(result, Is.EqualTo(_storedFiles.Count));
    }

    [Test]
    public async Task GetPaginatedFilesAsync_WhenPageSizeIsZero_ShouldReturnEmptyCollection()
    {
        // Arrange
        const int pageSize = 0;

        // Act
        var result = await _repository.GetPaginatedFilesAsync(0, pageSize);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetPaginatedFilesAsync_WhenPageIsOutOfRange_ShouldReturnEmptyCollection()
    {
        // Arrange
        const int pageSize = 5;
        const int page = 2;

        // Act
        var result = await _repository.GetPaginatedFilesAsync(page, pageSize);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetPaginatedFilesAsync_WhenPageIsWithinRange_ShouldReturnFiles()
    {
        // Arrange
        const int pageSize = 2;
        const int page = 0;

        // Act
        var result = await _repository.GetPaginatedFilesAsync(page, pageSize);

        // Assert
        Assert.That(result, Is.EquivalentTo([_storedFiles[0], _storedFiles[1]]).UsingPropertiesComparer());
    }

    [Test]
    public async Task GetPaginatedFilesAsync_WhenLastPage_ShouldReturnFiles()
    {
        // Arrange
        const int pageSize = 2;
        const int page = 1;

        // Act
        var result = await _repository.GetPaginatedFilesAsync(page, pageSize);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(_storedFiles[2].Id));
    }
}

using Api.Data;
using Api.Data.Entities;
using Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Api.Tests.Repositories;

public class FileRepositoryTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private readonly ApplicationDbContext _context;
    private readonly FileRepository _repository;
    private static readonly User _user = new() { UserName = "user" };
    private static readonly List<Tag> _tags = [new() { Name = "tag1" }];

    private const string ExpectedName = "name";
    private static readonly StoredFile _file = new()
    {
        Id = 1,
        Name = ExpectedName,
        ContentType = "text/plain",
        Path = "path/to/file",
        Uploader = _user,
        Tags = _tags,
    };

    public FileRepositoryTests()
    {
        _context = CreateContext();
        _repository = new FileRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddFileToDatabase()
    {
        // Act
        await _repository.AddAsync(_file);

        // Assert
        var savedFile = await _context.StoredFiles.FindAsync(_file.Id);
        savedFile.Should().NotBeNull();
        savedFile!.Name.Should().Be(ExpectedName);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnFile()
    {
        // Arrange
        await _context.StoredFiles.AddAsync(_file);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(_file.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(ExpectedName);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllByTagAsync_WithMatchingTag_ShouldReturnFiles()
    {
        // Arrange
        await _context.StoredFiles.AddAsync(_file);
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.GetAllByTagAsync("tag1");

        // Assert
        results.Should().HaveCount(1);
        results[0].Name.Should().Be(ExpectedName);
    }

    [Fact]
    public async Task GetAllByTagAsync_WithNonMatchingTag_ShouldReturnEmptyList()
    {
        // Arrange
        await _context.StoredFiles.AddAsync(_file);
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.GetAllByTagAsync("non-existing-tag");

        // Assert
        results.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetPaginatedFilesAsync_WithValidParameters_ShouldReturnPaginatedFiles()
    {
        // Arrange
        var files = new List<StoredFile>
        {
            new() { Id = 2, Name = "File1", ContentType = "text/plain", Path = "path/to/file1", Uploader = _user, Tags = _tags },
            new() { Id = 3, Name = "File2", ContentType = "text/plain", Path = "path/to/file2", Uploader = _user, Tags = _tags },
            new() { Id = 4, Name = "File3", ContentType = "text/plain", Path = "path/to/file3", Uploader = _user, Tags = _tags }
        };

        await _context.StoredFiles.AddRangeAsync(files);
        await _context.SaveChangesAsync();

        const int page = 0;
        const int pageSize = 2;

        // Act
        var result = await _repository.GetPaginatedFilesAsync(page, pageSize);

        // Assert
        result.Should().HaveCount(pageSize);
    }
}

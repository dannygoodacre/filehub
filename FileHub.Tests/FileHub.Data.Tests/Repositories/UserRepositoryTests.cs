using FileHub.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FileHub.Data.Tests.Repositories;

[TestFixture]
public class UserRepositoryTests : TestBase
{
    private ApplicationContext _context = null!;

    private UserRepository _repository = null!;

    [SetUp]
    public async Task SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationContext(options);

        _context.Users.AddRange(new List<ApplicationUser>
        {
            new() { Id = 123, UserName = "test_username1", JoinedAt = new DateTime(2021, 11, 25) },
            new() { Id = 456, UserName = "test_username2", JoinedAt = new DateTime(2020, 03, 05) },
        });

        await _context.SaveChangesAsync();

        _repository = new UserRepository(_context);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _context.DisposeAsync();
    }

    [Test]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        const int id = 123;

       // Act
        var result = await _repository.GetByIdAsync(id);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result?.Id, Is.EqualTo(123));
            Assert.That(result?.Name, Is.EqualTo("test_username1"));
            Assert.That(result?.JoinedAt, Is.EqualTo(new DateTime(2021, 11, 25)));
        }
    }

    [Test]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        const int id = 321;

        // Act
        var result = await _repository.GetByIdAsync(id);

        // Assert
        Assert.That(result, Is.Null);
    }
}

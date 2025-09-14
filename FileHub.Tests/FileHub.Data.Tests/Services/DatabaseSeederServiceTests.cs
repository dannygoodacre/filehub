using FileHub.Data.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace FileHub.Data.Tests.Services;

[TestFixture]
public class DatabaseSeederServiceTests : TestBase
{
    private Mock<UserManager<ApplicationUser>> _userManagerMock = null!;

    private const string Username = "test_username";

    private const string Password = "test_password";

    private readonly ApplicationUser _user = new() { UserName = Username };

    private readonly IConfiguration _configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            { "SeedUser:Username", Username },
            { "SeedUser:Password", Password }
        }!)
        .Build();

    private DatabaseSeederService _service = null!;

    [SetUp]
    public void SetUp()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>(MockBehavior.Strict);

        _userManagerMock = new Mock<UserManager<ApplicationUser>>(MockBehavior.Strict,
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _userManagerMock
            .SetupSet(x => x.Logger = null!)
            .Verifiable(Times.Once);

        _service = new DatabaseSeederService(_userManagerMock.Object, _configuration);
    }

    [Test]
    public async Task SeedAsync_WhenUserExists_ShouldNotCreateUser()
    {
        // Arrange
        _userManagerMock
            .Setup(x => x.FindByNameAsync(Username))
            .ReturnsAsync(_user)
            .Verifiable(Times.Once);

        // Act
        await _service.SeedAsync();
    }

    [Test]
    public async Task SeedAsync_WhenUserDoesNotExist_ShouldCreateUser()
    {
        // Arrange
        _userManagerMock
            .Setup(x => x.FindByNameAsync(Username))
            .ReturnsAsync(null as ApplicationUser)
            .Verifiable(Times.Once);

        _userManagerMock
            .Setup(x => x.CreateAsync(
                It.Is<ApplicationUser>(y => y.UserName == Username),
                It.Is<string>(y => y == Password)))
            .ReturnsAsync(IdentityResult.Success)
            .Verifiable(Times.Once);

        // Act
        await _service.SeedAsync();
    }
}

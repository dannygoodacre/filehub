using FileHub.Data.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;

namespace FileHub.Data.Tests.Services;

[TestFixture]
public class IdentityServiceTests : TestBase
{
    private Mock<IOptions<IdentityOptions>> _optionsMock;

    private Mock<IUserStore<ApplicationUser>> _userStoreMock;

    private Mock<UserManager<ApplicationUser>> _userManagerMock;

    private Mock<SignInManager<ApplicationUser>> _signInManagerMock;

    private IdentityService _service;

    private const int TestUserId = 123;

    private const string TestUsername = "test_username";

    private const string TestPassword = "test_password";

    private const string TestOldPassword = "test_old_password";

    private const string TestNewPassword = "test_new_password";

    private ApplicationUser _testUser = new()
    {
        Id = TestUserId,
        UserName = TestUsername,
        EmailConfirmed = true
    };

    [SetUp]
    public void SetUp()
    {
        _optionsMock = new Mock<IOptions<IdentityOptions>>(MockBehavior.Strict);

        _userStoreMock = new Mock<IUserStore<ApplicationUser>>(MockBehavior.Strict);

        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            MockBehavior.Strict,
            _userStoreMock.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        _userManagerMock
            .SetupSet(x => x.Logger = null!)
            .Verifiable(Times.Once);

        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            MockBehavior.Strict,
            _userManagerMock.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            null!, null!, null!, null!);

        _signInManagerMock
            .SetupSet(x => x.Logger = null!)
            .Verifiable(Times.Once);

        _service = new IdentityService(
            _optionsMock.Object,
            _userStoreMock.Object,
            _userManagerMock.Object,
            _signInManagerMock.Object);
    }

    [Test]
    public async Task RegisterAsync_WhenUserCreationFails_ShouldReturnFailure()
    {
        // Arrange
        Setup_UserStore_SetUserNameAsync();

        _userManagerMock
            .Setup(x => x.CreateAsync(
                It.IsAny<ApplicationUser>(),
                It.Is<string>(y => y == TestPassword)))
            .ReturnsAsync(IdentityResult.Failed())
            .Verifiable(Times.Once);

        // Act
        var result = await _service.RegisterAsync(TestUsername, TestPassword);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo("Registration failed."));
        }
    }

    [Test]
    public async Task RegisterAsync_WhenUserCreationSucceeds_ShouldReturnSuccess()
    {
        // Arrange
        Setup_UserStore_SetUserNameAsync();

        Setup_UserManager_CreateAsync();

        var dateTimeBeforeTest = DateTime.UtcNow;

        // Act
        var result = await _service.RegisterAsync(TestUsername, TestPassword);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);

            Assert.That(_testUser.UserName, Is.EqualTo(TestUsername));

            Assert.That(_testUser.JoinedAt, Is.GreaterThanOrEqualTo(dateTimeBeforeTest));
            Assert.That(_testUser.JoinedAt, Is.LessThanOrEqualTo(DateTime.UtcNow));
        }
    }

    [Test]
    public async Task LoginAsync_WhenUserIsNull_ShouldReturnUserNotFound()
    {
        _testUser = null!;

        Setup_UserManager_FindByNameAsync();

        // Act
        var result = await _service.LoginAsync(TestUsername, TestPassword);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo("User not found."));
        }
    }

    [Test]
    public async Task LoginAsync_WhenUserAccountNotConfirmedAndNeeded_ShouldReturnAccountNotConfirmed()
    {
        // Arrange
        Setup_UserManager_FindByNameAsync();

        Setup_Options();

        _userManagerMock
            .Setup(x => x.IsEmailConfirmedAsync(
                It.Is<ApplicationUser>(y => y == _testUser)))
            .ReturnsAsync(false)
            .Verifiable(Times.Once);

        // Act
        var result = await _service.LoginAsync(TestUsername, TestPassword);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo("User not confirmed."));
        }
    }

    [Test]
    public async Task LoginAsync_WhenUserAccountConfirmedAndNeededAndSignInFailed_ShouldReturnLoginFailed()
    {
        // Arrange
        Setup_UserManager_FindByNameAsync();

        Setup_Options();

        Setup_UserManager_IsEmailConfirmedAsync();

        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync(
                It.Is<string>(y => y == TestUsername),
                It.Is<string>(y => y == TestPassword),
                It.Is<bool>(y => y == true),
                It.Is<bool>(y => y == false)))
            .ReturnsAsync(SignInResult.Failed)
            .Verifiable(Times.Once);

        // Act
        var result = await _service.LoginAsync(TestUsername, TestPassword);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo("Login failed."));
        }
    }

    [Test]
    public async Task LoginAsync_WhenSignInSucceeds_ShouldReturnSuccess()
    {
        // Arrange
        Setup_UserManager_FindByNameAsync();

        Setup_Options();

        Setup_UserManager_IsEmailConfirmedAsync();

        Setup_SignInManager_PasswordSignInAsync();

        // Act
        var result = await _service.LoginAsync(TestUsername, TestPassword);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task LogoutAsync_WhenUserSignsOut_ShouldReturnSuccess()
    {
        // Arrange
        Setup_SignInManager_SignOutAsync();

        // Act
        var result = await _service.LogoutAsync();

        // Assert
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task GetUserInfoAsync_WhenUserIsNull_ShouldReturnUserNotFound()
    {
        // Arrange
        _userManagerMock
            .Setup(x => x.FindByIdAsync(TestUserId.ToString()))
            .ReturnsAsync(null as ApplicationUser)
            .Verifiable(Times.Once);

        // Act
        var result = await _service.GetUserInfoAsync(TestUserId);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo("User not found."));
        }
    }

    [Test]
    public async Task GetUserInfoAsync_WhenUserFound_ShouldReturnSuccess()
    {
        // Arrange
        Setup_UserManager_FindByIdAsync();

        // Act
        var result = await _service.GetUserInfoAsync(TestUserId);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value?.Username, Is.EqualTo(TestUsername));
            Assert.That(result.Value?.IsAccountConfirmed, Is.True);
        }
    }

    [Test]
    public async Task ChangePasswordAsync_WhenUserIsNull_ShouldReturnUserNotFound()
    {
        // Arrange
        _userManagerMock
            .Setup(x => x.FindByIdAsync(TestUserId.ToString()))
            .ReturnsAsync(null as ApplicationUser)
            .Verifiable(Times.Once);

        // Act
        var result = await _service.ChangePasswordAsync(TestUserId, TestOldPassword, TestNewPassword);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo("User not found."));
        }
    }

    [TestCase(null!)]
    [TestCase("")]
    [TestCase(" ")]
    public async Task ChangePasswordAsync_WhenOldPasswordIsEmpty_ShouldReturnUserNotFound(string oldPassword)
    {
        // Arrange
        Setup_UserManager_FindByIdAsync();

        // Act
        var result = await _service.ChangePasswordAsync(TestUserId, oldPassword, TestNewPassword);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo("Old password cannot be empty."));
        }
    }

    [TestCase(null!)]
    [TestCase("")]
    [TestCase(" ")]
    public async Task ChangePasswordAsync_WhenNewPasswordIsEmpty_ShouldReturnUserNotFound(string newPassword)
    {
        // Arrange
        Setup_UserManager_FindByIdAsync();

        // Act
        var result = await _service.ChangePasswordAsync(TestUserId, TestOldPassword, newPassword);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo("New password cannot be empty."));
        }
    }

    [Test]
    public async Task ChangePasswordAsync_WhenChangePasswordFails_ShouldReturnFailure()
    {
        // Arrange
        Setup_UserManager_FindByIdAsync();

        _userManagerMock
            .Setup(x => x.ChangePasswordAsync(
                It.Is<ApplicationUser>(y => y == _testUser),
                It.Is<string>(y => y == TestOldPassword),
                It.Is<string>(y => y == TestNewPassword)))
            .ReturnsAsync(IdentityResult.Failed())
            .Verifiable(Times.Once);

        // Act
        var result = await _service.ChangePasswordAsync(TestUserId, TestOldPassword, TestNewPassword);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo("Password change failed."));
        }
    }

    [Test]
    public async Task ChangePasswordAsync_WhenChangePasswordSucceeds_ShouldReturnSuccess()
    {
        // Arrange
        Setup_UserManager_FindByIdAsync();

        Setup_UserManager_ChangePasswordAsync();

        // Act
        var result = await _service.ChangePasswordAsync(TestUserId, TestOldPassword, TestNewPassword);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
    }

    private void Setup_Options(int times = 1)
    {
        _optionsMock.Setup(x => x.Value)
            .Returns(new IdentityOptions()
            {
                SignIn = new SignInOptions()
                {
                    RequireConfirmedAccount = true
                }
            })
            .Verifiable(Times.Exactly(times));
    }

    private void Setup_UserManager_ChangePasswordAsync(int times = 1)
    {
        _userManagerMock
            .Setup(x => x.ChangePasswordAsync(
                It.Is<ApplicationUser>(y => y ==_testUser),
                It.Is<string>(y => y == TestOldPassword),
                It.Is<string>(y => y == TestNewPassword)))
            .ReturnsAsync(IdentityResult.Success)
            .Verifiable(Times.Exactly(times));
    }

    private void Setup_UserManager_CreateAsync(int times = 1)
    {
        _userManagerMock
            .Setup(x => x.CreateAsync(
                It.IsAny<ApplicationUser>(),
                It.Is<string>(y => y == TestPassword)))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationUser, string>(
                (applicationUser, _) => _testUser = applicationUser)
            .Verifiable(Times.Exactly(times));
    }

    private void Setup_UserManager_FindByIdAsync(int times = 1)
    {
        _userManagerMock
            .Setup(x => x.FindByIdAsync(
                It.Is<string>(y => y == TestUserId.ToString())))
            .ReturnsAsync(_testUser)
            .Verifiable(Times.Exactly(times));
    }

    private void Setup_UserManager_FindByNameAsync(int times = 1)
    {
        _userManagerMock
            .Setup(x => x.FindByNameAsync(
                It.Is<string>(y => y == TestUsername)))
            .ReturnsAsync(_testUser)
            .Verifiable(Times.Exactly(times));
    }

    private void Setup_UserManager_IsEmailConfirmedAsync(int times = 1)
    {
        _userManagerMock
            .Setup(x => x.IsEmailConfirmedAsync(
                It.Is<ApplicationUser>(y => y == _testUser)))
            .ReturnsAsync(true)
            .Verifiable(Times.Exactly(times));
    }

    private void Setup_UserStore_SetUserNameAsync(int times = 1)
    {
        _userStoreMock
            .Setup(x => x.SetUserNameAsync(
                It.IsAny<ApplicationUser>(),
                It.Is<string?>(y => y == TestUsername),
                It.Is<CancellationToken>(y => y == CancellationToken.None)))
            .Callback<ApplicationUser, string?, CancellationToken>(
                (applicationUser, _, _) => applicationUser.UserName = TestUsername)
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(times));
    }

    private void Setup_SignInManager_PasswordSignInAsync(int times = 1)
    {
        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync(
                It.Is<string>(y => y == TestUsername),
                It.Is<string>(y => y == TestPassword),
                It.Is<bool>(y => y == true),
                It.Is<bool>(y => y == false)))
            .ReturnsAsync(SignInResult.Success)
            .Verifiable(Times.Exactly(times));
    }

    private void Setup_SignInManager_SignOutAsync(int times = 1)
    {
        _signInManagerMock
            .Setup(x => x.SignOutAsync())
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(times));
    }
}

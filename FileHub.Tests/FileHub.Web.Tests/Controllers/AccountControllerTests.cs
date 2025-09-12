using FileHub.Core.Common;
using FileHub.Core.Identity;
using FileHub.Data.Services;
using FileHub.Web.Controllers;
using FileHub.Web.Models;
using FileHub.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace FileHub.Web.Tests.Controllers;

[TestFixture]
public class AccountControllerTests : TestBase
{
    private Mock<ICurrentUserService> _mockCurrentUserService;

    private Mock<IIdentityService> _mockIdentityService;

    private AccountController _controller;

    private bool _testIsAccountConfirmed;

    private string _testNewPassword;

    private string _testOldPassword;

    private string _testPassword;

    private int _testUserId;

    private UserInfo _testUserInfo;

    private string _testUsername;

    [SetUp]
    public void SetUp()
    {
        _mockCurrentUserService = new Mock<ICurrentUserService>(MockBehavior.Strict);

        _mockIdentityService = new Mock<IIdentityService>(MockBehavior.Strict);

        _controller = new AccountController(_mockCurrentUserService.Object, _mockIdentityService.Object);

        _testIsAccountConfirmed = true;

        _testNewPassword = "Test New Password";

        _testOldPassword = "Test Old Password";

        _testPassword = "Test Password";

        _testUserId = 123;

        _testUserInfo = new UserInfo
        {
            Username = _testUsername,
            IsAccountConfirmed = _testIsAccountConfirmed
        };

        _testUsername = "Test Username";
    }

    [Test]
    public async Task RegisterAsync_WhenSuccess_ShouldReturnOk()
    {
        // Arrange
        _mockIdentityService
            .Setup(x => x.RegisterAsync(
                It.Is<string>(y => y == _testUsername),
                It.Is<string>(y => y == _testPassword)))
            .ReturnsAsync(Result.Success())
            .Verifiable(Times.Once);

        var loginRequest = new RegistrationRequest()
        {
            Username = _testUsername,
            Password = _testPassword
        };

        // Act
        var result = await _controller.RegisterAsync(loginRequest);

        // Assert
        Assert.That(result, Is.EqualTo(Results.Ok()));
    }

    [Test]
    public async Task RegisterAsync_WhenFailure_ShouldReturnInternalServerError()
    {
        // Arrange
        _mockIdentityService
            .Setup(x => x.RegisterAsync(
                It.Is<string>(y => y == _testUsername),
                It.Is<string>(y => y == _testPassword)))
            .ReturnsAsync(Result.InternalError("Test Error"))
            .Verifiable(Times.Once);

        var loginRequest = new RegistrationRequest()
        {
            Username = _testUsername,
            Password = _testPassword
        };

        // Act
        var result = await _controller.RegisterAsync(loginRequest);

        // Assert
        Assert.That(result, Is.EqualTo(Results.InternalServerError()));
    }

    [Test]
    public async Task LoginAsync_WhenSuccess_ShouldReturnOk()
    {
        // Arrange
        _mockIdentityService
            .Setup(x => x.LoginAsync(
                It.Is<string>(y => y == _testUsername),
                It.Is<string>(y => y == _testPassword)))
            .ReturnsAsync(Result.Success())
            .Verifiable(Times.Once);

        var loginRequest = new LoginRequest()
        {
            Username = _testUsername,
            Password = _testPassword
        };

        // Act
        var result = await _controller.LoginAsync(loginRequest);

        // Assert
        Assert.That(result, Is.EqualTo(Results.Ok()));
    }

    [Test]
    public async Task LoginAsync_WhenFailure_ShouldReturnInternalServerError()
    {
        // Arrange
        _mockIdentityService
            .Setup(x => x.LoginAsync(
                It.Is<string>(y => y == _testUsername),
                It.Is<string>(y => y == _testPassword)))
            .ReturnsAsync(Result.InternalError("Test Error"))
            .Verifiable(Times.Once);

        var loginRequest = new LoginRequest()
        {
            Username = _testUsername,
            Password = _testPassword
        };

        // Act
        var result = await _controller.LoginAsync(loginRequest);

        // Assert
        Assert.That(result, Is.EqualTo(Results.InternalServerError()));
    }

    [Test]
    public async Task LogoutAsync_WhenSuccess_ShouldReturnOk()
    {
        // Arrange
        _mockIdentityService
            .Setup(x => x.LogoutAsync())
            .ReturnsAsync(Result.Success())
            .Verifiable(Times.Once);

        // Act
        var result = await _controller.LogoutAsync();

        // Assert
        Assert.That(result, Is.EqualTo(Results.Ok()));
    }

    [Test]
    public async Task LogoutAsync_WhenFailure_ShouldReturnInternalServerError()
    {
        // Arrange
        _mockIdentityService
            .Setup(x => x.LogoutAsync())
            .ReturnsAsync(Result.InternalError("Test Error"))
            .Verifiable(Times.Once);

        // Act
        var result = await _controller.LogoutAsync();

        // Assert
        Assert.That(result, Is.EqualTo(Results.InternalServerError()));
    }

    [Test]
    public async Task GetInfoAsync_WhenSuccess_ShouldReturnOk()
    {
        // Arrange
        Setup_CurrentUserService_GetCurrentUserId();

        _mockIdentityService
            .Setup(x => x.GetUserInfoAsync(
                It.Is<int>(y => y == _testUserId)))
            .ReturnsAsync(Result<UserInfo>.Success(_testUserInfo))
            .Verifiable(Times.Once);

        // Act
        var result = await _controller.GetInfoAsync();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.TypeOf<Ok<UserInfo>>());

            var okResult = result as Ok<UserInfo>;

            Assert.That(okResult?.Value, Is.EqualTo(_testUserInfo));
        }
    }

    [Test]
    public async Task GetInfoAsync_WhenFailure_ShouldReturnBadRequest()
    {
        // Arrange
        Setup_CurrentUserService_GetCurrentUserId();

        _mockIdentityService
            .Setup(x => x.GetUserInfoAsync(
                It.Is<int>(y => y == _testUserId)))
            .ReturnsAsync(Result<UserInfo>.DomainError("Test domain error"))
            .Verifiable(Times.Once);

        // Act
        var result = await _controller.GetInfoAsync();

        // Assert
        Assert.That(result, Is.TypeOf<BadRequest<string>>());

        var badResult = result as BadRequest<string>;

        Assert.That(badResult?.Value, Is.EqualTo("Test domain error"));
    }

    [Test]
    public async Task ChangePasswordAsync_WhenSuccess_ShouldReturnOk()
    {
        // Arrange
        Setup_CurrentUserService_GetCurrentUserId();

        var passwordUpdateRequest = new PasswordUpdateRequest()
        {
            NewPassword = _testNewPassword,
            OldPassword = _testOldPassword
        };

        _mockIdentityService
            .Setup(x => x.ChangePasswordAsync(
                It.Is<int>(y => y == _testUserId),
                It.Is<string>(y => y == _testOldPassword),
                It.Is<string>(y => y == _testNewPassword)))
            .ReturnsAsync(Result.Success())
            .Verifiable(Times.Once);

        // Act
        var result = await _controller.ChangePasswordAsync(passwordUpdateRequest);

        // Assert
        Assert.That(result, Is.EqualTo(Results.Ok()));
    }

    [Test]
    public async Task ChangePasswordAsync_WhenFailed_ShouldReturnInternalServerError()
    {
        // Arrange
        Setup_CurrentUserService_GetCurrentUserId();

        var passwordUpdateRequest = new PasswordUpdateRequest()
        {
            NewPassword = _testNewPassword,
            OldPassword = _testOldPassword
        };

        _mockIdentityService
            .Setup(x => x.ChangePasswordAsync(
                It.Is<int>(y => y == _testUserId),
                It.Is<string>(y => y == _testOldPassword),
                It.Is<string>(y => y == _testNewPassword)))
            .ReturnsAsync(Result.InternalError("Test Error"))
            .Verifiable(Times.Once);

        // Act
        var result = await _controller.ChangePasswordAsync(passwordUpdateRequest);

        // Assert
        Assert.That(result, Is.EqualTo(Results.InternalServerError()));
    }

    private void Setup_CurrentUserService_GetCurrentUserId()
    {
        _mockCurrentUserService
            .Setup(x => x.GetCurrentUserId())
            .Returns(_testUserId)
            .Verifiable(Times.Once);
    }
}

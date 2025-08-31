using System.Security.Claims;
using System.Security.Principal;
using FileHub.Data;
using FileHub.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace FileHub.Web.Tests.Services;

[TestFixture]
public class CurrentUserServiceTests : TestBase
{
    private Mock<IHttpContextAccessor> _mockHttpContextAccessor;

    private Mock<IUserStore<ApplicationUser>> _mockUserStore;

    private CurrentUserService _service;

    [SetUp]
    public void SetUp()
    {
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>(MockBehavior.Strict);

        _mockUserStore = new Mock<IUserStore<ApplicationUser>>(MockBehavior.Strict);

        _service = new CurrentUserService(_mockHttpContextAccessor.Object);
    }

    [Test]
    public void GetCurrentUserId_WhenHttpContextNull_ShouldReturnZero()
    {
        // Arrange
        _mockHttpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(null as HttpContext)
            .Verifiable(Times.Once);

        // Act
        var result = _service.GetCurrentUserId();

        // Assert
        Assert.That(result, Is.Zero);
    }

    [Test]
    public void GetCurrentUserId_WhenUserIdentityIsNull_ShouldReturnZero()
    {
        // Arrange
        var claimsPrincipalMock = new Mock<ClaimsPrincipal>(MockBehavior.Strict);

        claimsPrincipalMock
            .Setup(p => p.Identity)
            .Returns(null as IIdentity)
            .Verifiable(Times.Once);

        var httpContextMock = new Mock<HttpContext>(MockBehavior.Strict);

        httpContextMock
            .Setup(c => c.User)
            .Returns(claimsPrincipalMock.Object);

        _mockHttpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(httpContextMock.Object)
            .Verifiable(Times.Once);

        // Act
        var result = _service.GetCurrentUserId();

        // Assert
        Assert.That(result, Is.Zero);
    }

    [Test]
    public void GetCurrentUserId_WhenUserIsNotAuthenticated_ShouldReturnZero()
    {
        // Arrange
        var mockIdentity = new Mock<IIdentity>(MockBehavior.Strict);

        mockIdentity
            .Setup(x => x.IsAuthenticated)
            .Returns(false)
            .Verifiable(Times.Once);

        var claimsPrincipalMock = new Mock<ClaimsPrincipal>(MockBehavior.Strict);

        claimsPrincipalMock
            .Setup(x => x.Identity)
            .Returns(mockIdentity.Object)
            .Verifiable(Times.Exactly(2));

        var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);

        mockHttpContext
            .Setup(x => x.User)
            .Returns(claimsPrincipalMock.Object)
            .Verifiable(Times.Exactly(2));

        _mockHttpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(mockHttpContext.Object)
            .Verifiable(Times.Once);

        // Act
        var result = _service.GetCurrentUserId();

        // Assert
        Assert.That(result, Is.Zero);
    }

    [Test]
    public void GetCurrentUserId_WhenNameIdentifierIsNotAnInteger_ReturnsZero()
    {
        // Arrange
        var mockIdentity = new Mock<IIdentity>(MockBehavior.Strict);

        mockIdentity
            .Setup(i => i.IsAuthenticated)
            .Returns(true)
            .Verifiable(Times.Once);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "abc")
        };

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));

        var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);

        mockHttpContext
            .Setup(c => c.User)
            .Returns(claimsPrincipal)
            .Verifiable(Times.Exactly(3));

        _mockHttpContextAccessor
            .Setup(h => h.HttpContext)
            .Returns(mockHttpContext.Object)
            .Verifiable(Times.Once);

        // Act
        var result = _service.GetCurrentUserId();

        // Assert
        Assert.That(result, Is.Zero);
    }

    [Test]
    public void GetCurrentUserId_WhenValid_ReturnsCurrentUserId()
    {
        // Arrange
        var mockIdentity = new Mock<IIdentity>(MockBehavior.Strict);

        mockIdentity
            .Setup(i => i.IsAuthenticated)
            .Returns(true)
            .Verifiable(Times.Once);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "123")
        };

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));

        var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);

        mockHttpContext
            .Setup(c => c.User)
            .Returns(claimsPrincipal)
            .Verifiable(Times.Exactly(3));

        _mockHttpContextAccessor
            .Setup(h => h.HttpContext)
            .Returns(mockHttpContext.Object)
            .Verifiable(Times.Once);

        // Act
        var result = _service.GetCurrentUserId();

        // Assert
        Assert.That(result, Is.EqualTo(123));
    }
}

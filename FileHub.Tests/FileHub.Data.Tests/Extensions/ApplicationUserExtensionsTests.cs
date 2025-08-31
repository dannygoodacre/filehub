using FileHub.Data.Extensions;

namespace FileHub.Data.Tests.Extensions;

[TestFixture]
public class ApplicationUserExtensionsTests : TestBase
{
    [Test]
    public void ToUser()
    {
        // Arrange
        var applicationUser = new ApplicationUser()
        {
            Id = 123,
            UserName = "test_username",
            JoinedAt = new DateTime(2020, 10, 12),
        };

        // Act
        var user = applicationUser.ToUser();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(user.Id, Is.EqualTo(applicationUser.Id));
            Assert.That(user.Name, Is.EqualTo(applicationUser.UserName));
            Assert.That(user.JoinedAt, Is.EqualTo(applicationUser.JoinedAt));
        }
    }
}

using FileHub.Application.Abstractions.Data;
using FileHub.Application.Abstractions.Data.Repositories;
using FileHub.Data.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileHub.Data.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests : TestBase
{
    [Test]
    public void AddData()
    {
        // Arrange
        var services = new ServiceCollection();

        IConfiguration configuration = new ConfigurationBuilder().Build();

        // Act
        services.AddData(configuration);

        var provider = services.BuildServiceProvider();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(provider.GetService<IApplicationContext>(), Is.Not.Null);
            Assert.That(provider.GetService<IFileRepository>(), Is.Not.Null);
            Assert.That(provider.GetService<ITagRepository>(), Is.Not.Null);
            Assert.That(provider.GetService<IUserRepository>(), Is.Not.Null);
        }
    }

    [Test]
    public void AddIdentity()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddDbContext<ApplicationContext>();

        services.AddAuthentication();

        // Act
        services.AddIdentity();

        var provider = services.BuildServiceProvider();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(provider.GetService<UserManager<ApplicationUser>>(), Is.Not.Null);
            Assert.That(provider.GetService<SignInManager<ApplicationUser>>(), Is.Not.Null);
        }
    }
}

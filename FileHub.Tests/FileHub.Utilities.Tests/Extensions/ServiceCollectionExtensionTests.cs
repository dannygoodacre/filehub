using FileHub.Application.Abstractions.Services;
using FileHub.Utilities.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileHub.Utilities.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionTests : TestBase
{
    [Test]
    public void AddUtilities()
    {
        // Arrange
        var services = new ServiceCollection();

        IConfiguration configuration = new ConfigurationBuilder().Build();

        // Act
        services.AddUtilities(configuration);

        var provider = services.BuildServiceProvider();

        // Assert
        Assert.That(provider.GetService<IIdEncoderService<int>>(), Is.Not.Null);
    }
}

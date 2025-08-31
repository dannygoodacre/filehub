using FileHub.Application.Abstractions.Services;
using FileHub.Storage.Configuration;
using FileHub.Storage.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FileHub.Storage.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionTests : TestBase
{
    [Test]
    public void AddStorage()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddLogging();

        IConfiguration configuration = new ConfigurationBuilder().Build();

        services.AddSingleton(configuration);

        // Act
        services.AddStorage(configuration);

        var provider = services.BuildServiceProvider();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(provider.GetService<IValidateOptions<FileStorageOptions>>(), Is.Not.Null);

            Assert.That(provider.GetService<IOptions<FileStorageOptions>>(), Is.Not.Null);

            Assert.That(provider.GetService<IFileStorageService>(), Is.Not.Null);
        }
    }
}

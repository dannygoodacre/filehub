using FileHub.Application.Abstractions.Data;
using FileHub.Application.Abstractions.Data.Repositories;
using FileHub.Application.Abstractions.Services;
using FileHub.Application.Commands;
using FileHub.Application.Extensions;
using FileHub.Application.Queries;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FileHub.Application.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests : TestBase
{
    [Test]
    public void AddApplication()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddSingleton(Mock.Of<ITagRepository>());
        services.AddSingleton(Mock.Of<IFileRepository>());

        services.AddSingleton(Mock.Of<IApplicationContext>());

        services.AddSingleton(Mock.Of<IFileStorageService>());
        services.AddSingleton(Mock.Of<IFileLocationService>());

        services.AddSingleton(Mock.Of<IIdEncoderService<int>>());

        services.AddLogging();

        // Act
        services.AddApplication();

        var provider = services.BuildServiceProvider();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(provider.GetService<IAddFile>(), Is.Not.Null);
            Assert.That(provider.GetService<IGetFileContent>(), Is.Not.Null);
            Assert.That(provider.GetService<IGetFileMetadata>(), Is.Not.Null);
        }
    }
}

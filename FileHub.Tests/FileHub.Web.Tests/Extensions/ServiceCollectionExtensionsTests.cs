using FileHub.Application.Abstractions.Services;
using FileHub.Web.Configuration;
using FileHub.Web.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FileHub.Web.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests : TestBase
{
    [Test]
    public void AddCustomAuthentication()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCustomAuthentication();

        var provider = services.BuildServiceProvider();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(provider.GetService<IOptions<CookiePolicyOptions>>()?.Value, Is.Not.Null);
            Assert.That(provider.GetService<IOptions<AuthenticationOptions>>()?.Value, Is.Not.Null);
        }

        var cookieOptionsMonitor = provider.GetService<IOptionsMonitor<CookieAuthenticationOptions>>();

        Assert.That(cookieOptionsMonitor, Is.Not.Null);

        var cookieOptions = cookieOptionsMonitor.Get(IdentityConstants.ApplicationScheme);

        Assert.That(cookieOptions, Is.Not.Null);
        Assert.That(cookieOptions.Events, Is.Not.Null);
    }

    [TestCase(null!)]
    [TestCase("")]
    [TestCase(" ")]
    public void AddCustomCors_WhenOriginEmpty_ShouldThrowInvalidOperationException(string origin)
    {
        // Arrange
        var services = new ServiceCollection();

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "CorsAllowedOrigins", origin },
            }!)
            .Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => services.AddCustomCors(configuration));
    }

    [Test]
    public void AddCustomCors()
    {
        // Arrange
        var services = new ServiceCollection();

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "CorsAllowedOrigins", "Test CORS Allowed Origins 1; Test CORS Allowed Origins 2"},
            }!)
            .Build();

        // Act
        services.AddCustomCors(configuration);

        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetRequiredService<IOptions<CorsOptions>>().Value;

        Assert.That(options, Is.Not.Null);

        var policy = options.GetPolicy("Web");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(policy?.Origins.Contains("test cors allowed origins 1"), Is.True);
            Assert.That(policy?.Origins.Contains("test cors allowed origins 2"), Is.True);
            Assert.That(policy?.SupportsCredentials, Is.True);
        }
    }

    [Test]
    public void AddSwaggerDocumentation()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSwaggerDocumentation();
        var provider = services.BuildServiceProvider();

        // Get SwaggerGenOptions
        var swaggerOptions = provider.GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(swaggerOptions.SwaggerGeneratorOptions.SwaggerDocs, Does.ContainKey("v1"));
            Assert.That(swaggerOptions.SwaggerGeneratorOptions.SwaggerDocs["v1"].Title, Is.EqualTo("FileHub"));
            Assert.That(swaggerOptions.SwaggerGeneratorOptions.SwaggerDocs["v1"].Version, Is.EqualTo("v0.1"));
        }
    }

    [Test]
    public void AddWebServices()
    {
        // Arrange
        var services = new ServiceCollection();

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RootUrl", "Test Root Url" }
            })
            .Build();

        // Act
        services.AddWebServices(configuration);

        var provider = services.BuildServiceProvider();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(provider.GetService<IOptions<WebOptions>>()?.Value, Is.Not.Null);
            Assert.That(provider.GetService<IValidateOptions<WebOptions>>(), Is.Not.Null);
            Assert.That(provider.GetService<IFileLocationService>(), Is.Not.Null);
        }
    }
}

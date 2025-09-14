using FileHub.Application.Abstractions.Services;
using FileHub.Core.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using FileHub.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileHub.API.Tests;

public class FileHubWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    public List<StoredFile> StoredFiles { get; } = [];

    public List<string> ExternalFileIds { get; } = [];

    public List<Category> Categories { get; private set; } = [];

    public List<Tag> Tags { get; set; } = [];

    private readonly string _contentDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString());

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationContext>>();
            services.RemoveAll<ApplicationContext>();

            var connection = new SqliteConnection("DataSource=:memory:");

            connection.Open();

            services.AddDbContext<ApplicationContext>(options => options.UseSqlite(connection));

            services.AddAuthentication(defaultScheme: "TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    "TestScheme", _ => { });

            var provider = services.BuildServiceProvider();

            CreateContentDirectory();

            SeedTestData(provider);
        });

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var options = new Dictionary<string, string?>
            {
                ["ContentDirectory"] = _contentDirectoryPath
            };

            config.AddInMemoryCollection(options);
        });
    }

    private void SeedTestData(ServiceProvider provider)
    {
        using var scope = provider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        context.Database.Migrate();

        Categories =
        [
            new Category { Name = "Category 1" },
            new Category { Name = "Category 2" }
        ];

        context.Categories.AddRange(Categories);

        Tags =
        [
            new Tag { Name = "Tag 1" },
            new Tag { Name = "Tag 2" }
        ];

        context.Tags.AddRange(Tags);

        var filePath = Path.Combine(_contentDirectoryPath, "test_file.txt");

        File.WriteAllText(filePath, "Test File Content");

        var storedFile = context.StoredFiles.Add(new StoredFile
        {
            Name = "Test stored file 1",
            StorageKey = filePath,
            ContentType = "text/plain",
            Category = Categories[0],
            CreatedAt = DateTime.UtcNow,
            UserId = 1,
            Tags = [Tags[0], Tags[1]]
        }).Entity;

        StoredFiles.Add(storedFile);

        for (var i = 2; i < 10; i++)
        {
            storedFile = context.StoredFiles.Add(new StoredFile
            {
                Name = $"Test stored file {i}",
                StorageKey = $"Test storage key {i}",
                ContentType = "test/content-type",
                Category = Categories[0],
                CreatedAt = DateTime.UtcNow,
                UserId = 1,
                Tags = [Tags[0]]
            }).Entity;

            StoredFiles.Add(storedFile);
        }

        for (var i = 10; i < 15; i++)
        {
            storedFile = context.StoredFiles.Add(new StoredFile
            {
                Name = $"Test stored file {i}",
                StorageKey = $"Test storage key {i}",
                ContentType = "test/content-type",
                Category = Categories[1],
                CreatedAt = DateTime.UtcNow,
                UserId = 1,
                Tags = [Tags[0]]
            }).Entity;

            StoredFiles.Add(storedFile);
        }

        context.SaveChanges();

        var idEncoderService = scope.ServiceProvider.GetRequiredService<IIdEncoderService<int>>();

        var externalIds = StoredFiles.Select(x => x.Id).Select(idEncoderService.Encode);

        ExternalFileIds.AddRange(externalIds!);
    }

    private void CreateContentDirectory()
    {
        if (!Directory.Exists(_contentDirectoryPath))
        {
            Directory.CreateDirectory(_contentDirectoryPath);
        }
    }

    public new void Dispose()
    {
        if (Directory.Exists(_contentDirectoryPath))
        {
            Directory.Delete(_contentDirectoryPath, true);
        }

        base.Dispose();
    }
}

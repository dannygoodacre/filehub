using FileHub.Core.Entities;
using FileHub.Data;
using Microsoft.EntityFrameworkCore;

namespace FileHub.Tests.Seeder;

class Program
{
    private const string TestFilesDirectory = "./TestFiles";

    static async Task Main()
    {
        const string databasePath = "../../data/filehub.db";

        var fileStorageDirectory = Path.GetFullPath("../../files");

        const string dockerFilesDirectory = "/app/files";

        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;

        await using var context = new ApplicationContext(options);

        await context.Database.EnsureCreatedAsync();

        await context.Database.ExecuteSqlRawAsync("DELETE FROM StoredFileTag");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Tags");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM StoredFiles");

        var tags = new List<Tag>
        {
            new() { Name = "Tag 1" },
            new() { Name = "Tag 2" },
            new() { Name = "Tag 3" },
            new() { Name = "Tag 4" },
            new() { Name = "Tag 5" }
        };

        for (var i = 0; i < 3; i++)
        {
            await context.StoredFiles.AddRangeAsync(GetStoredFiles(dockerFilesDirectory, tags));
        }

        await context.SaveChangesAsync();

        foreach (var file in Directory.EnumerateFiles(fileStorageDirectory))
        {
            File.Delete(file);
        }

        foreach (var file in Directory.EnumerateFiles(TestFilesDirectory))
        {
            File.Copy(file, $"{fileStorageDirectory}/{Path.GetFileName(file)}");
        }
    }

    private static List<StoredFile> GetStoredFiles(string fileStoragePath, List<Tag> tags)
        =>
        [
            new()
            {
                Name = "Image 1",
                ContentType = "image/png",
                StorageKey = $"{fileStoragePath}/image1.png",
                CreatedAt = DateTime.UtcNow,
                Tags = tags[..1],
                UserId = 1,
            },

            new()
            {
                Name = "Image 2",
                ContentType = "image/png",
                StorageKey = $"{fileStoragePath}/image2.png",
                CreatedAt = DateTime.UtcNow,
                Tags = tags[..2],
                UserId = 1,
            },

            new()
            {
                Name = "Image 3",
                ContentType = "image/png",
                StorageKey = $"{fileStoragePath}/image3.png",
                CreatedAt = DateTime.UtcNow,
                Tags = tags[..3],
                UserId = 1,
            },

            new()
            {
                Name = "Text file 1",
                ContentType = "text/plain",
                StorageKey = $"{fileStoragePath}/text1.txt",
                CreatedAt = DateTime.UtcNow,
                Tags = tags[..4],
                UserId = 1,
            },

            new()
            {
                Name = "Text file 2",
                ContentType = "text/plain",
                StorageKey = $"{fileStoragePath}/text2.txt",
                CreatedAt = DateTime.UtcNow,
                Tags = tags[..5],
                UserId = 1,
            }
        ];
}

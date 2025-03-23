using Api.Data.Entities;

namespace Api.Factories;

public static class StoredFileFactory
{
    /// <summary>
    /// Create a stored file with the given properties.
    /// </summary>
    public static StoredFile Create(string name, string path, string contentType, User user, List<Tag>? tags) =>
        new()
        {
            Name = name,
            Path = path,
            ContentType = contentType,
            CreatedAt = DateTime.UtcNow,
            Uploader = user,
            Tags = tags,
        };
}

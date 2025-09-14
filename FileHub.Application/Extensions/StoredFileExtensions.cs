using FileHub.Core.Entities;
using FileHub.Core.Files;

namespace FileHub.Application.Extensions;

internal static class StoredFileExtensions
{
    public static FileMetadata ToMetadata(this StoredFile file, string externalId, string accessLocation)
        => new()
        {
            Id = externalId,
            Name = file.Name,
            AccessLocation = accessLocation,
            ContentType = file.ContentType,
            CreatedAt = file.CreatedAt,
            Category = file.Category.Name,
            Tags = file.Tags.Select(x => x.Name).ToList()
        };
}

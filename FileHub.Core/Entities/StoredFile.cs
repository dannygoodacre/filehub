namespace FileHub.Core.Entities;

public class StoredFile
{
    public int Id { get; init; }

    public required string Name { get; init; }

    public required string StorageKey { get; init; }

    public required string ContentType { get; init; }

    public int CategoryId { get; init; }

    public required Category Category { get; init; }

    public required ICollection<Tag> Tags { get; init; }

    public required DateTime CreatedAt { get; init; }

    public required int UserId { get; init; }
}

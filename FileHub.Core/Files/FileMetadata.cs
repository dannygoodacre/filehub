namespace FileHub.Core.Files;

public class FileMetadata
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string AccessLocation { get; init; }

    public required string ContentType { get; init; }

    public required DateTime CreatedAt { get; init; }

    public required string Category { get; init; }

    public required List<string> Tags { get; init; }
}

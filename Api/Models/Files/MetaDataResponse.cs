namespace Api.Models.Files;

/// <summary>
/// Data transfer object for file metadata.
/// </summary>
public class MetaDataResponse
{
    public required string Name { get; init; }
    public required string Url { get; init; }
    public required string ContentType { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required string Uploader { get; init; }
    public ICollection<string>? Tags { get; init; }
}

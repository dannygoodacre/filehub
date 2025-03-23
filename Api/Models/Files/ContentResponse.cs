namespace Api.Models.Files;

/// <summary>
/// Data transfer object for returning file content.
/// </summary>
public class ContentResponse
{
    public required byte[] Data { get; init; }
    public required string ContentType { get; init; }
}

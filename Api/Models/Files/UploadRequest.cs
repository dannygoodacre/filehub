namespace Api.Models.Files;

/// <summary>
/// Data transfer object for file upload.
/// </summary>
public class UploadRequest
{
    public required IFormFile File { get; init; }
    public required string Name { get; init; }
    public List<string>? Tags { get; init; }
}

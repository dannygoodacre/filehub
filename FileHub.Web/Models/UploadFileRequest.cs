namespace FileHub.Web.Models;

/// <summary>
/// Data transfer object for uploading a file.
/// </summary>
public class UploadFileRequest
{
    /// <summary>
    /// File.
    /// </summary>
    public required IFormFile File { get; init; }

    /// <summary>
    /// File name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Category.
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Tags.
    /// </summary>
    public required List<string>? Tags { get; init; }
}

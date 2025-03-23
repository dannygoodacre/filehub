namespace Api.Services;

public interface IStorageService
{
    /// <summary>
    /// Save a file to the given path on disk.
    /// </summary>
    Task SaveFileToPathAsync(IFormFile file, string path);

    /// <summary>
    /// Create a relative path to the file.
    /// </summary>
    string CreateFilePath(IFormFile file);

    /// <summary>
    /// Create a unique file name, formed of a timestamp and a random string.
    /// </summary>
    string CreateFileName(IFormFile file);

    /// <summary>
    /// Return if a file is not null and not empty.
    /// </summary>
    bool IsValidFile(IFormFile file);

    /// <summary>
    /// Get the directory where files are stored.
    /// </summary>
    string GetFileDirectory();
}

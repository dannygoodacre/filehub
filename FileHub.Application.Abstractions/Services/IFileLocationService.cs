namespace FileHub.Application.Abstractions.Services;

/// <summary>
/// Provides a method for getting the location at which a user may access a file.
/// </summary>
public interface IFileLocationService
{
    /// <summary>
    /// Get the unique location at which the file can be accessed: e.g. a URL.
    /// </summary>
    /// <param name="fileId">The unique ID of the file.</param>
    /// <returns></returns>
    public string GetFileAccessLocation(string fileId);
}

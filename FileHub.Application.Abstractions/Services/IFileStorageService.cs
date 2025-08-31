using FileHub.Core.Common;

namespace FileHub.Application.Abstractions.Services;

/// <summary>
/// Provides methods for saving and reading file content from stored files.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Save the file to persistent storage.
    /// </summary>
    /// <param name="content">The file content to be saved.</param>
    /// <param name="extension">The file extension.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    /// <returns>A <see cref="Result{T}"/> containing the unique key to access the file, if successful.</returns>
    Task<Result<string>> SaveAsync(Stream content, string extension, CancellationToken cancellationToken = default);

    /// <summary>
    /// Open a shared, read-only stream to the saved file.
    /// </summary>
    /// <param name="key">The unique storage key of the file.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    /// <returns>A <see cref="Result{T}"/> containing a <see cref="Stream"/> of the file's content, if successful.</returns>
    Task<Result<Stream>> OpenReadStreamAsync(string key, CancellationToken cancellationToken = default);
}

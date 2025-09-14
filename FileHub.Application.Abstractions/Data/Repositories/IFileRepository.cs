using FileHub.Core.Entities;

namespace FileHub.Application.Abstractions.Data.Repositories;

/// <summary>
/// A repository for stored files.
/// </summary>
public interface IFileRepository
{
    /// <summary>
    /// Insert a file.
    /// </summary>
    /// <param name="storedFile">The file to insert.</param>
    void Add(StoredFile storedFile);

    /// <summary>
    /// Retrieve a file.
    /// </summary>
    /// <param name="id">The unique ID of the file.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    /// <returns>A <see cref="StoredFile"/>, if found; otherwise, null.</returns>
    Task<StoredFile?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve all files with a specified tag.
    /// </summary>
    /// <param name="tagName">The tag's name.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="StoredFile"/> instances.</returns>
    Task<List<StoredFile>> GetAllByTagAsync(string tagName, CancellationToken cancellationToken = default);


    /// <summary>
    /// Retrieve the total number of files.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    /// <returns>The total number of files.</returns>
    Task<int> GetFilesCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve a subset of files using pagination.
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve (zero-based).</param>
    /// <param name="pageSize">The number of files to include per page.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="StoredFile"/> instances.</returns>
    Task<List<StoredFile>> GetPaginatedFilesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve a subset of files with a given category using pagination.
    /// </summary>
    /// <param name="categoryId">The ID of the category.</param>
    /// <param name="pageNumber">The page number to retrieve (zero-based).</param>
    /// <param name="pageSize">The number of files to include per page.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="StoredFile"/> instances.</returns>
    Task<List<StoredFile>> GetPaginatedFilesByCategoryAsync(int categoryId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}

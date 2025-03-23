using Api.Data.Entities;

namespace Api.Repositories;

public interface IFileRepository
{
    /// <summary>
    /// Insert a file.
    /// </summary>
    Task AddAsync(StoredFile storedFile);

    /// <summary>
    /// Retrieve a file.
    /// </summary>
    Task<StoredFile?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieve all files with a specified tag.
    /// </summary>
    Task<List<StoredFile>> GetAllByTagAsync(string tagName);

    /// <summary>
    /// Retrieve a subset of files using pagination.
    /// </summary>
    /// <param name="page">The page number to retrieve (zero-based).</param>
    /// <param name="pageSize">The number of files to include per page.</param>
    Task<List<StoredFile>> GetPaginatedFilesAsync(int page, int pageSize);
}

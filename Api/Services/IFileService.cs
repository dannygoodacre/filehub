using Api.Data.Entities;
using Api.Models.Common;
using Api.Models.Files;

namespace Api.Services;

public interface IFileService
{
    /// <summary>
    /// Create a new file instance, write it to disk, and store it in the database.
    /// </summary>
    /// <returns>
    /// <list type="bullet">
    ///     <item><see cref="Result.Success"/></item>
    ///     <item><see cref="Result.Failure"/> with <see cref="Error.NoFileUploaded"/>.</item>
    ///     <item><see cref="Result.Failure"/> with <see cref="Error.FileStorageError"/>.</item>
    /// </list>
    /// </returns>
    public Task<Result> AddFileAsync(UploadRequest fileUploadRequest, User user);

    /// <summary>
    /// Fetch the content of a file.
    /// </summary>
    /// <returns>
    /// <list type="bullet">
    ///     <item>A <see cref="ContentResponse"/> instance.</item>
    ///     <item><see cref="Maybe{T}.None"/> if the file is not found or the given ID is invalid.</item>
    /// </list>
    /// </returns>
    public Task<Maybe<ContentResponse>> GetFileContentByIdAsync(int id);

    /// <summary>
    /// Fetch the metadata of a file by its ID.
    /// </summary>
    /// <returns>
    /// <list type="bullet">
    ///     <item>A <see cref="MetaDataResponse"/> instance.</item>
    ///     <item><see cref="Maybe{T}.None"/> if the file is not found or the given ID is invalid.</item>
    /// </list>
    /// </returns>
    public Task<Maybe<MetaDataResponse>> GetFileMetaDataByIdAsync(int id);

    /// <summary>
    /// Fetch all files with a given tag.
    /// </summary>
    /// <returns>
    /// <list type="bullet">
    ///     <item><see cref="Result.Failure"/> with <see cref="Error.InvalidTagName"/>.</item>
    ///     <item><see cref="Result.Failure"/> with <see cref="Error.TagNotFound"/>.</item>
    ///     <item><see cref="Result.Success"/> with <see cref="Maybe{T}.None"/> if the tag exists but no matching files are found.</item>
    ///     <item><see cref="Result.Success"/> with a list of <see cref="MetaDataResponse"/> instances.</item>
    /// </list>
    /// </returns>
    public Task<Result<Maybe<List<MetaDataResponse>>>> GetAllFilesByTagAsync(string tagName);

    /// <summary>
    /// Fetch a subset of files using pagination.
    /// </summary>
    /// <param name="page">The page number to retrieve (zero-based).</param>
    /// <param name="pageSize">The number of files to include per page.</param>
    /// <returns>
    /// <list type="bullet">
    ///     <item><see cref="Result.Failure"/> with <see cref="Error.InvalidPage"/>.</item>
    ///     <item><see cref="Result.Success"/> with a list of <see cref="MetaDataResponse"/> instances.</item>
    /// </list>
    /// </returns>
    public Task<Result<List<MetaDataResponse>>> GetPaginatedFilesAsync(int page, int pageSize);
}

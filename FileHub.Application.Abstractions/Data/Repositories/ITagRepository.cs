using FileHub.Core.Entities;

namespace FileHub.Application.Abstractions.Data.Repositories;

/// <summary>
/// A repository for tags.
/// </summary>
public interface ITagRepository
{
    /// <summary>
    /// Insert a collection of tags.
    /// </summary>
    void AddRange(IEnumerable<Tag> tags);

    /// <summary>
    /// Retrieve all tags with the given names, ignoring any that do not exist.
    /// </summary>
    /// <param name="tagNames">The names of the tags.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    Task<List<Tag>> GetManyForUpdateAsync(IEnumerable<string> tagNames, CancellationToken cancellationToken = default);

    /// <summary>
    /// Return true if a tag with the given name exists, else false.
    /// </summary>
    /// <param name="tagName">The name of the tag.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    Task<bool> ExistsAsync(string tagName, CancellationToken cancellationToken = default);
}

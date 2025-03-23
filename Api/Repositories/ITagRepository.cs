using Api.Data.Entities;

namespace Api.Repositories;

public interface ITagRepository
{
    /// <summary>
    /// Insert a collection of tags.
    /// </summary>
    Task AddRangeAsync(IEnumerable<Tag> tags);

    /// <summary>
    /// Retrieve all tags.
    /// </summary>
    Task<IEnumerable<Tag>> GetAllAsync();

    /// <summary>
    /// Retrieve all tags corresponding to the given tag names.
    /// </summary>
    Task<IEnumerable<Tag>> GetTagsByNamesAsync(IEnumerable<string> tagNames);

    /// <summary>
    /// Return whether a tag with the given name exists.
    /// </summary>
    Task<bool> TagExistsByNameAsync(string tagName);
}

using Api.Data.Entities;

namespace Api.Services;

public interface ITagService
{
    /// <summary>
    /// Return all tags with the given names, creating and inserting new ones if they don't exist.
    /// </summary>
    public Task<IEnumerable<Tag>> GetOrCreateTagsByNameAsync(List<string> tagNames);

    /// <summary>
    /// Return whether a tag with the given name exists.
    /// </summary>
    public Task<bool> TagExistsByNameAsync(string tagName);
}

using Api.Data.Entities;
using Api.Repositories;

namespace Api.Services;

public class TagService(ITagRepository tagRepository) : ITagService
{
    public async Task<IEnumerable<Tag>> GetOrCreateTagsByNameAsync(List<string> tagNames)
    {
        // Get all tag names which already exist.
        var existingTags = (await tagRepository.GetTagsByNamesAsync(tagNames)).ToList();
        var existingTagNames = existingTags.Select(t => t.Name).ToHashSet();

        // Create new tags for all tag names which don't yet exist.
        var newTags = tagNames
            .Where(name => !existingTagNames.Contains(name))
            .Select(name => new Tag { Name = name })
            .ToList();

        if (newTags.Count == 0)
            return existingTags;

        await tagRepository.AddRangeAsync(newTags);
        return existingTags.Concat(newTags);
    }

    public Task<bool> TagExistsByNameAsync(string tagName)
    {
        return tagRepository.TagExistsByNameAsync(tagName);
    }
}

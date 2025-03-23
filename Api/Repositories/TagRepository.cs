using Api.Data;
using Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class TagRepository(ApplicationDbContext context) : ITagRepository
{
    public async Task AddRangeAsync(IEnumerable<Tag> tags)
    {
        context.Tags.AddRange(tags);
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Tag>> GetAllAsync() =>
        await context.Tags.AsNoTracking().ToListAsync();

    public async Task<IEnumerable<Tag>> GetTagsByNamesAsync(IEnumerable<string> tagNames) => 
        await context.Tags.Where(t => tagNames.Contains(t.Name)).ToListAsync();

    public Task<bool> TagExistsByNameAsync(string tagName) =>
        context.Tags.AnyAsync(t => t.Name == tagName);
}

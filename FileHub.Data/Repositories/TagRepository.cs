using FileHub.Application.Abstractions.Data.Repositories;
using FileHub.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileHub.Data.Repositories;

internal class TagRepository(ApplicationContext context) : ITagRepository
{
    public void AddRange(IEnumerable<Tag> tags) => context.Tags.AddRange(tags);

    public async Task<List<Tag>> GetManyForUpdateAsync(IEnumerable<string> tagNames, CancellationToken cancellationToken = default)
        => await context.Tags
            .AsTracking()
            .Where(t => tagNames.Contains(t.Name))
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(string tagName, CancellationToken cancellationToken = default)
        => context.Tags
            .AnyAsync(t => t.Name == tagName, cancellationToken);
}

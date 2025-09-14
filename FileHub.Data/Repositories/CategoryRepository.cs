using FileHub.Application.Abstractions.Data.Repositories;
using FileHub.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileHub.Data.Repositories;

internal class CategoryRepository(ApplicationContext context) : ICategoryRepository
{
    public void Add(Category category)
        => context.Categories
            .Add(category);

    public Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        => context.Categories
            .ToListAsync(cancellationToken);

    public Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        => context.Categories
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);

    public Task<Category?> GetByNameForUpdateAsync(string name, CancellationToken cancellationToken = default)
        => context.Categories
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);

    public Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)
        => context.Categories
            .AnyAsync(x => x.Name == name, cancellationToken);
}

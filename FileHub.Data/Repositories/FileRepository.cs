using FileHub.Application.Abstractions.Data.Repositories;
using FileHub.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileHub.Data.Repositories;

internal class FileRepository(ApplicationContext context) : IFileRepository
{
    public void Add(StoredFile storedFile) => context.StoredFiles.Add(storedFile);

    public async Task<StoredFile?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.StoredFiles
            .Include(x => x.Tags)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<List<StoredFile>> GetAllByTagAsync(string tagName, CancellationToken cancellationToken = default)
        => await context.StoredFiles
            .Include(x => x.Tags)
            .Where(x => x.Tags.Any(t => t.Name == tagName))
            .ToListAsync(cancellationToken: cancellationToken);

    public async Task<int> GetFilesCountAsync(CancellationToken cancellationToken = default)
        => await context.StoredFiles.CountAsync(cancellationToken);

    public async Task<List<StoredFile>> GetPaginatedFilesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        => await context.StoredFiles
            .OrderBy(x => x.Id)
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .Include(x => x.Tags)
            .ToListAsync(cancellationToken: cancellationToken);
}

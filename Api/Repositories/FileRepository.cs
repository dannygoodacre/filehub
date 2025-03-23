using Api.Data;
using Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class FileRepository(ApplicationDbContext context) : IFileRepository
{
    public async Task AddAsync(StoredFile storedFile)
    {
        await context.AddAsync(storedFile);
        await context.SaveChangesAsync();
    }

    public async Task<StoredFile?> GetByIdAsync(int id) =>
        await context
            .StoredFiles.AsNoTracking()
            .Include(sf => sf.Uploader)
            .Include(sf => sf.Tags)
            .FirstOrDefaultAsync(sf => sf.Id == id);

    public async Task<List<StoredFile>> GetAllByTagAsync(string tagName) =>
        await context
            .StoredFiles.AsNoTracking()
            .Include(sf => sf.Uploader)
            .Include(sf => sf.Tags)
            .Where(sf => sf.Tags.Any(t => t.Name == tagName))
            .ToListAsync();

    public async Task<List<StoredFile>> GetPaginatedFilesAsync(int page, int pageSize) =>
        await context
            .StoredFiles.AsNoTracking()
            .OrderBy(sf => sf.Id)
            .Skip(page * pageSize)
            .Take(pageSize)
            .Include(sf => sf.Uploader)
            .Include(sf => sf.Tags)
            .ToListAsync();
}

namespace Api.Services;

public class StorageService(IConfiguration configuration) : IStorageService
{
    public async Task SaveFileToPathAsync(IFormFile file, string path)
    {
        await using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);
    }

    public string CreateFilePath(IFormFile file) =>
        Path.Combine(GetFileDirectory(), CreateFileName(file));

    public string CreateFileName(IFormFile file) =>
        $"{DateTime.UtcNow:yyyyMMddHHmm}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

    public bool IsValidFile(IFormFile? file) => file is not null && file.Length != 0;

    public string GetFileDirectory() => configuration["FileDirectory"]!;
}

namespace FileHub.Storage.Configuration;

public class FileStorageOptions
{
    public required string ContentDirectory { get; init; }

    public int FileStreamBufferSize { get; init; } = 4096;
}

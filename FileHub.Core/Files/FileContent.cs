namespace FileHub.Core.Files;

public class FileContent
{
    public required string ContentType { get; init; }

    public required Stream Content { get; init; }
}

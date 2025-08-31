namespace FileHub.Core.Entities;

public class Tag
{
    public int Id { get; init; }

    public required string Name { get; init; }

    public ICollection<StoredFile>? StoredFiles { get; init; }
}

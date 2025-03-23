using System.ComponentModel.DataAnnotations;

namespace Api.Data.Entities;

public class StoredFile
{
    public int Id { get; init; }

    public required string Name { get; set; }
    public required string Path { get; init; }
    public required string ContentType { get; init; }
    public DateTime CreatedAt { get; init; }
    public required User Uploader { get; init; }
    public ICollection<Tag> Tags { get; set; } = [];
}

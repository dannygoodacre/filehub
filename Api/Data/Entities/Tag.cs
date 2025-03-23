namespace Api.Data.Entities;

public class Tag
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<StoredFile>? StoredFiles { get; set; }
}

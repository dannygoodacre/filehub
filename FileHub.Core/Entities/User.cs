namespace FileHub.Core.Entities;

public class User
{
    public int Id { get; init; }

    public required string Name { get; init; }

    public DateTime JoinedAt { get; init; }
}

namespace Api.Models.Identity;

/// <summary>
/// Data transfer object for user information.
/// </summary>
public sealed class InfoResponse
{
    public required string Id { get; init; }
    public required string Username { get; init; }
    public required bool IsAccountConfirmed { get; init; }
}

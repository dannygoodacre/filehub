namespace Api.Models.Identity;

/// <summary>
/// Data transfer object for registering a new user.
/// </summary>
public sealed class RegisterRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}

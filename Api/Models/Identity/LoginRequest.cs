namespace Api.Models.Identity;

/// <summary>
/// Data transfer object for login requests.
/// </summary>
public sealed class LoginRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}

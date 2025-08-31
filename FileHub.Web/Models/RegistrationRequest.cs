namespace FileHub.Web.Models;

/// <summary>
/// Data transfer object for registering a new user.
/// </summary>
public class RegistrationRequest
{
    /// <summary>
    /// Username.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// Password.
    /// </summary>
    public required string Password { get; init; }
}

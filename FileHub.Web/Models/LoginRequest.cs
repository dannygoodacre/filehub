namespace FileHub.Web.Models;

/// <summary>
/// Data transfer object for login requests.
/// </summary>
public class LoginRequest
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

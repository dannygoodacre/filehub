namespace FileHub.Web.Models;

/// <summary>
/// Data transfer object for updating a user's password.
/// </summary>
public class PasswordUpdateRequest
{
    /// <summary>
    /// New password.
    /// </summary>
    public required string NewPassword { get; init; }

    /// <summary>
    /// Old password.
    /// </summary>
    public required string OldPassword { get; init; }
}

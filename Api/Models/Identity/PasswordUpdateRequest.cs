namespace Api.Models.Identity;

/// <summary>
/// Data transfer object for updating a user's password.
/// </summary>
public sealed class PasswordUpdateRequest
{
    public required string NewPassword { get; init; }
    public required string OldPassword { get; init; }
}

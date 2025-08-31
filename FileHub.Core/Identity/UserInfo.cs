namespace FileHub.Core.Identity;

public class UserInfo
{
    public required string Username { get; init; }

    public required bool IsAccountConfirmed { get; init; }
}

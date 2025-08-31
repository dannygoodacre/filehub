using FileHub.Core.Entities;

namespace FileHub.Data.Extensions;

public static class ApplicationUserExtensions
{
    public static User ToUser(this ApplicationUser applicationUser)
        => new()
        {
            Id = applicationUser.Id,
            Name = applicationUser.UserName!,
            JoinedAt = applicationUser.JoinedAt
        };
}

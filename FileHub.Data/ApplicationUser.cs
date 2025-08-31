using Microsoft.AspNetCore.Identity;

namespace FileHub.Data;

public class ApplicationUser : IdentityUser<int>
{
    public DateTime JoinedAt { get; init; }
}

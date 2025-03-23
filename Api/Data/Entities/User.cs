using Microsoft.AspNetCore.Identity;

namespace Api.Data.Entities;

public class User : IdentityUser<int>
{
    public DateTime JoinedAt { get; set; }
}

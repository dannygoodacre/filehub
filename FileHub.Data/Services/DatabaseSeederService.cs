using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace FileHub.Data.Services;

internal class DatabaseSeederService(UserManager<ApplicationUser> userManager,
                                     IConfiguration configuration) : IDatabaseSeederService
{
    public async Task SeedAsync()
    {
        var username = configuration["SeedUser:Username"]!;
        var password = configuration["SeedUser:Password"]!;

        var user = await userManager.FindByNameAsync(username);

        if (user is not null)
        {
            return;
        }

        user = new ApplicationUser
        {
            UserName = username
        };

        _ = await userManager.CreateAsync(user, password);
    }
}

/// <summary>
/// Provides a method for seeding the database with initial data.
/// </summary>
public interface IDatabaseSeederService
{
    /// <summary>
    /// Populate the database with initial data.
    /// </summary>
    Task SeedAsync();
}

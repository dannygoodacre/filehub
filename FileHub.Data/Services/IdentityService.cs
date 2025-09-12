using FileHub.Core.Common;
using FileHub.Core.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace FileHub.Data.Services;

public class IdentityService(IOptions<IdentityOptions> options,
                               IUserStore<ApplicationUser> userStore,
                               UserManager<ApplicationUser> userManager,
                               SignInManager<ApplicationUser> signInManager) : IIdentityService
{
    public async Task<Result> RegisterAsync(string username, string password)
    {
        var user = new ApplicationUser { JoinedAt = DateTime.UtcNow };

        await userStore.SetUserNameAsync(user, username, CancellationToken.None);

        var result = await userManager.CreateAsync(user, password);

        return result.Succeeded
            ? Result.Success()
            : Result.DomainError(result.ToString());
    }

    public async Task<Result> LoginAsync(string username, string password)
    {
        var user = await userManager.FindByNameAsync(username);

        if (user is null)
        {
            return Result.DomainError("User not found.");
        }

        if (options.Value.SignIn.RequireConfirmedAccount && !await userManager.IsEmailConfirmedAsync(user))
        {
            return Result.DomainError("User not confirmed.");
        }

        signInManager.AuthenticationScheme = IdentityConstants.ApplicationScheme;

        var result = await signInManager.PasswordSignInAsync(username, password, true, false);

        return result.Succeeded
            ? Result.Success()
            : Result.DomainError(result.ToString());
    }

    public async Task<Result> LogoutAsync()
    {
        await signInManager.SignOutAsync();

        return Result.Success();
    }

    public async Task<Result<UserInfo>> GetUserInfoAsync(int id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());

        if (user is null)
        {
            return Result<UserInfo>.DomainError("User not found.");
        }

        var userInfo = new UserInfo
        {
            Username = user.UserName!,
            IsAccountConfirmed = user.EmailConfirmed
        };

        return Result<UserInfo>.Success(userInfo);
    }

    public async Task<Result> ChangePasswordAsync(int id, string oldPassword, string newPassword)
    {
        var user = await userManager.FindByIdAsync(id.ToString());

        if (user is null)
        {
            return Result.DomainError("User not found.");
        }

        var validationState = new ValidationState();

        if (string.IsNullOrWhiteSpace(oldPassword))
        {
            validationState.AddError(nameof(oldPassword), "Must not be null, empty, or whitespace .");
        }

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            validationState.AddError(nameof(newPassword), "Must not be null, empty, or whitespace .");
        }

        if (validationState.HasErrors)
        {
            return Result.Invalid(validationState);
        }

        var result = await userManager.ChangePasswordAsync(user, oldPassword, newPassword);

        return result.Succeeded
            ? Result.Success()
            : Result.DomainError(result.ToString());
    }
}

/// <summary>
/// Provides methods for user registration and authentication.
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Register a new user.
    /// </summary>
    /// <param name="username">The user's username.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome of the operation.</returns>
    Task<Result> RegisterAsync(string username, string password);

    /// <summary>
    /// Login the user.
    /// </summary>
    /// <param name="username">The user's username.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome of the operation.</returns>
    Task<Result> LoginAsync(string username, string password);

    /// <summary>
    /// Logout the user.
    /// </summary>
    /// <returns>A <see cref="Result"/> indicating the outcome of the operation.</returns>
    Task<Result> LogoutAsync();

    /// <summary>
    /// Fetch the information of the current user.
    /// </summary>
    /// <returns>A <see cref="Result{T}"/> containing a <see cref="UserInfo"/>, if successful.</returns>
    Task<Result<UserInfo>> GetUserInfoAsync(int id);

    /// <summary>
    /// Change the current user's password.
    /// </summary>
    /// <param name="id">The user's ID.</param>
    /// <param name="oldPassword">The user's current password.</param>
    /// <param name="newPassword">The user's new password.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome of the operation.</returns>
    Task<Result> ChangePasswordAsync(int id, string oldPassword, string newPassword);
}

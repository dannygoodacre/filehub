using System.Security.Claims;

namespace FileHub.Web.Services;

internal class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public int GetCurrentUserId()
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext?.User.Identity is null || !httpContext.User.Identity.IsAuthenticated)
        {
            return 0;
        }

        var id = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(id, out var result)
            ? result
            : 0;
    }
}

/// <summary>
/// A service for getting information about the current user.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Get the ID of the current user.
    /// </summary>
    /// <returns>The ID of the current user, if found; otherwise, zero.</returns>
    int GetCurrentUserId();
}

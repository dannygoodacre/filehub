using FileHub.Data.Services;
using FileHub.Web.Extensions;
using FileHub.Web.Models;
using FileHub.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileHub.Web.Controllers;

/// <summary>
/// Endpoints for handling user account operations.
/// </summary>
[ApiController]
[Route("account")]
public class AccountController(ICurrentUserService currentUserService, IIdentityService identityService) : ControllerBase
{
    /// <summary>
    /// Register a new user.
    /// </summary>
    [HttpPost("register")]
    public async Task<IResult> RegisterAsync([FromBody] RegistrationRequest registration)
    {
        var result = await identityService.RegisterAsync(registration.Username, registration.Password);

        return result.ToHttpResponse();
    }

    /// <summary>
    /// Login an existing user.
    /// </summary>
    [HttpPost("login")]
    public async Task<IResult> LoginAsync([FromBody] LoginRequest login)
    {
        var result = await identityService.LoginAsync(login.Username, login.Password);

        return result.ToHttpResponse();
    }

    /// <summary>
    /// Logout the current user.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IResult> LogoutAsync()
    {
       var result = await identityService.LogoutAsync();

       return result.ToHttpResponse();
    }

    /// <summary>
    /// Get information about a user.
    /// </summary>
    [Authorize]
    [HttpGet("info")]
    public async Task<IResult> GetInfoAsync()
    {
        var id = currentUserService.GetCurrentUserId();

        var result = await identityService.GetUserInfoAsync(id);

        return result.ToHttpResponse();
    }

    /// <summary>
    /// Change a user's password.
    /// </summary>
    [Authorize]
    [HttpPost("changepassword")]
    public async Task<IResult> ChangePasswordAsync([FromBody] PasswordUpdateRequest passwordUpdateRequest)
    {
        var id = currentUserService.GetCurrentUserId();

        var result = await identityService.ChangePasswordAsync(id, passwordUpdateRequest.OldPassword, passwordUpdateRequest.NewPassword);

        return result.ToHttpResponse();
    }
}

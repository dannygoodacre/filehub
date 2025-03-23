using System.Net;
using System.Security.Claims;
using Api.Data.Entities;
using Api.Models.Common;
using Api.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace Api.Controllers;

[ApiController]
[Route("account")]
public class AccountController(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IUserStore<User> userStore,
    IConfiguration configuration
) : ControllerBase
{
    [HttpPost("register")]
    [SwaggerResponse(HttpStatusCode.OK, typeof(OkResult))]
    [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "Registration failed.")]
    public async Task<IResult> Register([FromBody] RegisterRequest registration)
    {
        var username = registration.Username;
        var user = new User();
        await userStore.SetUserNameAsync(user, username, CancellationToken.None);
        var result = await userManager.CreateAsync(user, registration.Password);

        return result.Succeeded ? TypedResults.Ok() : CreateValidationProblem(result);
    }

    [HttpPost("login")]
    [SwaggerResponse(HttpStatusCode.NoContent, typeof(void))]
    [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(ProblemDetails), Description = "Login failed.")]
    public async Task<IResult> Login([FromBody] LoginRequest login, [FromQuery] bool? useCookies, 
        [FromQuery] bool? useSessionCookies)
    {
        var user = await userManager.FindByNameAsync(login.Username);
        var requireAccountConfirmation = configuration.GetValue<bool>("RequireAccountConfirmation");

        if (requireAccountConfirmation && user is not null && !await userManager.IsEmailConfirmedAsync(user)) 
            return TypedResults.Problem(
                Messages.AccountNotConfirmed,
                statusCode: StatusCodes.Status401Unauthorized
            );

        var useCookieScheme = (useCookies == true) || (useSessionCookies == true);
        var isPersistent = (useCookies == true) && (useSessionCookies != true);

        signInManager.AuthenticationScheme = useCookieScheme
            ? IdentityConstants.ApplicationScheme
            : IdentityConstants.BearerScheme;

        var result = await signInManager.PasswordSignInAsync(
            login.Username,
            login.Password,
            isPersistent,
            lockoutOnFailure: true
        );

        // The signInManager already produced the needed response in the form of a cookie or bearer token.
        if (result.Succeeded)
            return TypedResults.Empty;

        return TypedResults.Problem(
            result.ToString(),
            statusCode: StatusCodes.Status401Unauthorized
        );
    }

    [Authorize]
    [HttpPost("logout")]
    [SwaggerResponse(HttpStatusCode.OK, typeof(void))]
    public async Task<IResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Results.Ok();
    }

    [Authorize]
    [HttpGet("info")]
    [SwaggerResponse(HttpStatusCode.OK, typeof(InfoResponse))]
    [SwaggerResponse(HttpStatusCode.NotFound, typeof(NotFoundResult), Description = "User not found.")]
    public async Task<IResult> GetInfo()
    {
        if (await userManager.GetUserAsync(User) is not { } user)
            return TypedResults.NotFound();

        return TypedResults.Ok(await CreateInfoResponseAsync(user, userManager));
    }

    [Authorize]
    [HttpPost("changepassword")]
    [SwaggerResponse(HttpStatusCode.OK, typeof(OkResult))]
    [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationProblemDetails), Description = "Password change failed.")]
    [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(ProblemDetails), Description = "User is not authenticated.")]
    [SwaggerResponse(HttpStatusCode.NotFound, typeof(NotFoundResult), Description = "User not found.")]
    public async Task<IResult> ChangePassword([FromBody] PasswordUpdateRequest passwordUpdateRequest)
    {
        if (await userManager.GetUserAsync(User) is not { } user)
            return TypedResults.NotFound();

        if (string.IsNullOrEmpty(passwordUpdateRequest.OldPassword))
            return CreateValidationProblem(
                "OldPasswordRequired",
                Messages.OldPasswordRequired
            );

        if (string.IsNullOrEmpty(passwordUpdateRequest.NewPassword))
            return CreateValidationProblem(
                "NewPasswordRequired",
                Messages.NewPasswordRequired
            );

        var changePasswordResult = await userManager.ChangePasswordAsync(
            user,
            passwordUpdateRequest.OldPassword,
            passwordUpdateRequest.NewPassword
        );

        if (!changePasswordResult.Succeeded)
            return CreateValidationProblem(changePasswordResult);

        return TypedResults.Ok(await CreateInfoResponseAsync(user, userManager));
    }

    [Authorize]
    [HttpGet("roles")]
    [SwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<object>))]
    [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(void), Description = "User is not authenticated.")]
    public IResult GetRoles()
    {
        if (User.Identity is null || !User.Identity.IsAuthenticated)
            return Results.Unauthorized();

        var identity = User.Identity as ClaimsIdentity;
        var roles = identity
            .FindAll(identity.RoleClaimType)
            .Select(c => new
            {
                c.Issuer,
                c.OriginalIssuer,
                c.Type,
                c.Value,
                c.ValueType,
            });

        return TypedResults.Json(roles);
    }

    private static ValidationProblem CreateValidationProblem(string errorCode, string errorDescription) =>
        TypedResults.ValidationProblem(
            new Dictionary<string, string[]> { { errorCode, [errorDescription] } }
        );

    private static ValidationProblem CreateValidationProblem(IdentityResult result)
    {
        var errorDictionary = result
            .Errors.GroupBy(e => e.Code)
            .ToDictionary(
                group => group.Key,
                group => group.Select(e => e.Description).ToArray()
            );

        return TypedResults.ValidationProblem(errorDictionary);
    }

    private static async Task<InfoResponse> CreateInfoResponseAsync<TUser>(TUser user, UserManager<TUser> userManager)
        where TUser : class
    {
        return new InfoResponse
        {
            Id = (await userManager.GetUserIdAsync(user)),
            Username =
                await userManager.GetUserNameAsync(user)
                ?? throw new NotSupportedException("Users must have a username."),
            IsAccountConfirmed = await userManager.IsEmailConfirmedAsync(user),
        };
    }
}

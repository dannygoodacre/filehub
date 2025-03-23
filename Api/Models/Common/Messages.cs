namespace Api.Models.Common;

/// <summary>
/// Messages used in API responses.
/// </summary>
public static class Messages
{
    // File
    public const string FileUploaded = "File uploaded";
    public const string NoFileUploaded = "No file uploaded";
    public const string FileNotFound = "File not found";
    public const string InvalidPageRequested = "Invalid page requested";

    // Tag
    public const string InvalidTagName = "Invalid tag name";
    public const string TagNotFound = "Tag not found";

    // User
    public const string NotLoggedIn = "You are not logged in";
    public const string OldPasswordRequired = "Old password is required to change password";
    public const string NewPasswordRequired = "New password is required to change password";
    public const string AccountNotConfirmed = "Account is not confirmed. Please contact the system administrator";

    // Misc.
    public const string InternalServerError = "Internal server error";
    public const string UnhandledException = "An unhandled exception occurred while processing the request.";
}

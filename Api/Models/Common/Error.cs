namespace Api.Models.Common;

/// <summary>
/// Internal error codes.
/// </summary>
public enum Error
{
    NotFound,
    InvalidId,
    NoFileUploaded,
    InvalidTagName,
    FileStorageError,
    TagNotFound,
    InvalidPage
}

using Microsoft.Extensions.Options;

namespace FileHub.Storage.Configuration;

internal class FileStorageOptionsValidator : IValidateOptions<FileStorageOptions>
{
    public ValidateOptionsResult Validate(string? name, FileStorageOptions options)
    {
        var errors = new List<string>();

        if (options.FileStreamBufferSize <= 0)
        {
            errors.Add($"'{nameof(options.FileStreamBufferSize)}' must be greater than 0.");
        }

        if (string.IsNullOrWhiteSpace(options.ContentDirectory))
        {
            errors.Add($"'{nameof(options.ContentDirectory)}' must not be null, empty, or whitespace.");
        }
        else
        {
            try
            {
                var path = Path.IsPathRooted(options.ContentDirectory)
                    ? options.ContentDirectory
                    : Path.Combine(Directory.GetCurrentDirectory(), options.ContentDirectory);

                if (!Directory.Exists(path))
                {
                    errors.Add($"'{nameof(options.ContentDirectory)}' does not exist.");
                }
            }
            catch (Exception e)
            {
                errors.Add($"'{nameof(options.ContentDirectory)}' is invalid: {e.Message}");
            }
        }

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}

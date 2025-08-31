using Microsoft.Extensions.Options;
using Sqids;

namespace FileHub.Utilities.Configuration;

internal class SqidsOptionsValidator : IValidateOptions<SqidsOptions>
{
    public ValidateOptionsResult Validate(string? name, SqidsOptions options)
    {
        var errors = new List<string>();

        switch (options.MinLength)
        {
            case < 0:
                errors.Add($"'{nameof(options.MinLength)}' must be greater than or equal to 0.");
                break;
            case > 255:
                errors.Add($"'{nameof(options.MinLength)}' must be less than or equal to 255.");
                break;
        }

        if (options.Alphabet.Length < 5)
        {
            errors.Add($"'{nameof(options.Alphabet)}' must be at least 5 characters.");
        }

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}

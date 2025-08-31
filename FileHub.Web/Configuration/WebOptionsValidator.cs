using Microsoft.Extensions.Options;

namespace FileHub.Web.Configuration;

internal class WebOptionsValidator : IValidateOptions<WebOptions>
{
    public ValidateOptionsResult Validate(string? name, WebOptions options)
        => string.IsNullOrWhiteSpace(options.RootUrl) 
            ? ValidateOptionsResult.Fail($"{nameof(options.RootUrl)} cannot be null or whitespace.") 
            : ValidateOptionsResult.Success;
}

using System.Text.RegularExpressions;

namespace FileHub.Application.Extensions;

internal static partial class StringExtensions
{
    [GeneratedRegex(@"^([\w.+-]+)/([\w.+-]+)$")]
    private static partial Regex ValidMimeType();

    public static bool IsValidMimeType(this string mimeType) => ValidMimeType().IsMatch(mimeType);
}

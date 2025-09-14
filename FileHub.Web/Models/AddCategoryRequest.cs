namespace FileHub.Web.Models;

/// <summary>
/// Data transfer object for adding a new category.
/// </summary>
public class AddCategoryRequest
{
    /// <summary>
    /// Category.
    /// </summary>
    public required string Category { get; init; }
}

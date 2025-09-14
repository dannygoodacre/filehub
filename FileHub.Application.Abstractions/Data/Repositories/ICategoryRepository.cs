using FileHub.Core.Entities;

namespace FileHub.Application.Abstractions.Data.Repositories;

public interface ICategoryRepository
{
    /// <summary>
    /// Insert a category.
    /// </summary>
    /// <param name="category">The new category.</param>
    void Add(Category category);

    /// <summary>
    /// Retrieve all categories.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="Category"/> instances.</returns>
    Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve a category with the given name, if exists.
    /// </summary>
    /// <param name="name">The name of the category.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    /// <returns>A <see cref="Category"/>, if found; otherwise, null.</returns>
    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// With tracking, retrieve a category with the given name, if it exists.
    /// </summary>
    /// <param name="name">The name of the category.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    /// <returns>A <see cref="Category"/>, if found; otherwise, null.</returns>
    Task<Category?> GetByNameForUpdateAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Return whether a category with the given name exists.
    /// </summary>
    /// <param name="name">The category name.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    /// <returns>True, if the category exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default);
}

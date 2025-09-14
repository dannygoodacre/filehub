using FileHub.Application.Abstractions.Data;
using FileHub.Application.Abstractions.Data.Repositories;
using FileHub.Core.Common;
using FileHub.Core.Entities;
using Microsoft.Extensions.Logging;

namespace FileHub.Application.Commands;

internal sealed class AddCategory(ILogger<AddCategory> logger,
                                  ICategoryRepository repository,
                                  IApplicationContext context) : CommandHandler<AddCategoryCommand>(logger), IAddCategory
{
    protected override string Name => "Add Category";

    protected override void Validate(ValidationState validationState, AddCategoryCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.CategoryName))
        {
            validationState.AddError(nameof(command.CategoryName), "Must not be null, empty, or whitespace.");
        }
    }

    protected override async Task<Result> InternalExecuteAsync(AddCategoryCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Command '{Name}' started with category '{Category}'.", Name, command.CategoryName);

        if (await repository.ExistsAsync(command.CategoryName, cancellationToken))
        {
            logger.LogError("Command '{Name}' did not create the category '{Category}' as it already exists.", Name, command.CategoryName);

            return Result.DomainError("Category already exists.");
        }

        repository.Add(new Category
        {
            Name = command.CategoryName
        });

        const int expectedChanges = 1;

        var actualChanges = await context.SaveChangesAsync();

        if (actualChanges != expectedChanges)
        {
            logger.LogError("Command '{Command}' wrote an unexpected number of entities to the database for Category '{Category}': expected '{Expected}', actual '{Actual}'.", Name, command.CategoryName, expectedChanges, actualChanges);
        }

        return Result.Success();
    }

    public Task<Result> ExecuteAsync(string categoryName, CancellationToken cancellationToken)
        => ExecuteAsync(new AddCategoryCommand
            {
                CategoryName = categoryName
            },
            cancellationToken);
}

/// <summary>
/// A command to create a new category.
/// </summary>
public interface IAddCategory
{
    /// <summary>
    /// Execute the command to create a new category.
    /// </summary>
    /// <param name="categoryName">The category name.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome of the operation.</returns>
    Task<Result> ExecuteAsync(string categoryName, CancellationToken cancellationToken = default);
}

internal class AddCategoryCommand : ICommand
{
    public required string CategoryName { get; init; }
}

using FileHub.Application.Abstractions.Data.Repositories;
using FileHub.Core.Common;
using Microsoft.Extensions.Logging;

namespace FileHub.Application.Queries;

internal sealed class GetAllCategories(ILogger<GetAllCategories> logger,
                                       ICategoryRepository repository) : QueryHandler<GetAllCategoriesQuery, List<string>>(logger), IGetAllCategories
{
    protected override string Name => "Get All Categories";

    protected override void Validate(ValidationState validationState, GetAllCategoriesQuery query, CancellationToken cancellationToken)
    {
    }

    protected override async Task<Result<List<string>>> InternalExecuteAsync(GetAllCategoriesQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Query '{Name}' started.", Name);

        var categories = await repository.GetAllAsync(cancellationToken);

        var categoryNames = categories.Select(x => x.Name).ToList();

        return Result<List<string>>.Success(categoryNames);
    }

    public Task<Result<List<string>>> ExecuteAsync(CancellationToken cancellationToken = default)
        => ExecuteAsync(new GetAllCategoriesQuery(), cancellationToken);
}

/// <summary>
/// A query to get the names of all categories.
/// </summary>
public interface IGetAllCategories
{
    /// <summary>
    /// Execute the query to get the names of all categories.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    /// <returns>A <see cref="Result{T}"/> containing a <see cref="List{T}"/> of all category names.</returns>
    public Task<Result<List<string>>> ExecuteAsync(CancellationToken cancellationToken = default);
}

internal class GetAllCategoriesQuery : IQuery;

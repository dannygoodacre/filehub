using FileHub.Application.Abstractions.Data.Repositories;
using FileHub.Core.Common;
using Microsoft.Extensions.Logging;

namespace FileHub.Application.Queries;

internal sealed class GetFilePageCount(ILogger<GetFilePageCount> logger,
                                       IFileRepository repository) : QueryHandler<GetPageCountQuery, int>(logger), IGetFilePageCount
{
    protected override string Name => "Get File Page Count";

    protected override void Validate(ValidationState validationState, GetPageCountQuery query, CancellationToken cancellationToken)
    {
        if (query.PageSize <= 0)
        {
            validationState.AddError(nameof(query.PageSize), "Must be greater than 0.");
        }
    }

    protected override async Task<Result<int>> InternalExecuteAsync(GetPageCountQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Query '{Name}' started with page size '{PageSize}'.", Name, query.PageSize);

        var fileCount = await repository.GetFilesCountAsync(cancellationToken);

        var pageCount = (fileCount + query.PageSize - 1) / query.PageSize;

        return Result<int>.Success(pageCount);
    }

    public Task<Result<int>> ExecuteAsync(int pageSize, CancellationToken cancellationToken = default)
        => ExecuteAsync(new GetPageCountQuery
        {
            PageSize = pageSize
        },
        cancellationToken);
}

/// <summary>
/// A query to get the number of pages of files of a given size.
/// </summary>
public interface IGetFilePageCount
{
    /// <summary>
    /// Execute the query to get the number of pages of files of a given size.
    /// </summary>
    /// <param name="pageSize">The number of files per page.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    /// <returns>A <see cref="Result{T}"/> containing the number of pages, if successful.</returns>
    Task<Result<int>> ExecuteAsync(int pageSize, CancellationToken cancellationToken = default);
}

internal class GetPageCountQuery : IQuery
{
    public required int PageSize { get; init; }
}

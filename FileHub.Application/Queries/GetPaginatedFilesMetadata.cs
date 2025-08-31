using FileHub.Application.Abstractions.Data.Repositories;
using FileHub.Application.Abstractions.Services;
using FileHub.Application.Extensions;
using FileHub.Core.Common;
using FileHub.Core.Files;
using Microsoft.Extensions.Logging;

namespace FileHub.Application.Queries;

internal sealed class GetPaginatedFilesMetadata(ILogger<GetPaginatedFilesMetadata> logger,
                                                IFileRepository repository,
                                                IFileLocationService locationService,
                                                IIdEncoderService<int> idEncoderService) : QueryHandler<GetPaginatedFilesQuery, List<FileMetadata>>(logger), IGetPaginatedFileMetadata
{
    protected override string Name => "Get Paginated Files Metadata";

    protected override void Validate(ValidationState validationState, GetPaginatedFilesQuery query, CancellationToken cancellationToken)
    {
        if (query.PageNumber < 1)
        {
            validationState.AddError(nameof(query.PageNumber), "Must be greater than or equal to 1.");
        }

        if (query.PageSize is < 1 or > 100)
        {
            validationState.AddError(nameof(query.PageSize), "Must be between 1 and 100, inclusive.");
        }
    }

    protected override async Task<Result<List<FileMetadata>>> InternalExecuteAsync(GetPaginatedFilesQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Query '{Name}' started with page number '{PageNumber}', page count '{PageCount}'.", Name, query.PageNumber, query.PageSize);

        var files = await repository.GetPaginatedFilesAsync(query.PageNumber - 1, query.PageSize, cancellationToken);

        var metadata = files.Select(file =>
        {
            var externalId = idEncoderService.Encode(file.Id)!;

            var accessLocation = locationService.GetFileAccessLocation(externalId);

            return file.ToMetadata(externalId, accessLocation);
        }).ToList();

        logger.LogInformation("Query '{Name}' completed with page number '{PageNumber}', page count '{PageCount}'.", Name, query.PageNumber, query.PageSize);

        return Result<List<FileMetadata>>.Success(metadata);
    }

    public Task<Result<List<FileMetadata>>> ExecuteAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
        => ExecuteAsync(new GetPaginatedFilesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        }, cancellationToken);
}

/// <summary>
/// A query to get a paginated collection of file metadata.
/// </summary>
public interface IGetPaginatedFileMetadata
{
    /// <summary>
    /// Execute the query to get the paginated collection of file metadata.
    /// </summary>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    /// <returns>A <see cref="Result{T}"/> containing a <see cref="List{T}"/> of <see cref="FileMetadata"/>, if successful.</returns>
    Task<Result<List<FileMetadata>>> ExecuteAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}

internal class GetPaginatedFilesQuery : IQuery
{
    public required int PageNumber { get; init; }

    public int PageSize { get; init; }
}

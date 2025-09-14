using FileHub.Application.Abstractions.Data.Repositories;
using FileHub.Application.Abstractions.Services;
using FileHub.Application.Extensions;
using FileHub.Core.Common;
using FileHub.Core.Entities;
using FileHub.Core.Files;
using Microsoft.Extensions.Logging;

namespace FileHub.Application.Queries;

internal sealed class GetPaginatedFilesMetadata(ILogger<GetPaginatedFilesMetadata> logger,
                                                IFileRepository repository,
                                                ICategoryRepository categoryRepository,
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

        if (query.Category is not null && string.IsNullOrWhiteSpace(query.Category))
        {
            validationState.AddError(nameof(query.Category), "Must not be empty or whitespace.");
        }
    }

    protected override async Task<Result<List<FileMetadata>>> InternalExecuteAsync(GetPaginatedFilesQuery query, CancellationToken cancellationToken)
    {
        if (query.Category is null)
        {
            logger.LogInformation("Query '{Name}' started with page number '{PageNumber}', page count '{PageCount}'.", Name, query.PageNumber, query.PageSize);
        }
        else
        {
            logger.LogInformation("Query '{Name}' started with page number '{PageNumber}', page count '{PageCount}', category '{Category}'.", Name, query.PageNumber, query.PageSize, query.Category);
        }

        List<StoredFile> files;

        if (query.Category is not null)
        {
            var category = await categoryRepository.GetByNameAsync(query.Category, cancellationToken);

            if (category is null)
            {
                logger.LogError("Query '{Name}' could not find category '{Category}'.", Name, query.Category);

                return Result<List<FileMetadata>>.DomainError("Category not found.");
            }

            files = await repository.GetPaginatedFilesByCategoryAsync(category.Id, query.PageNumber - 1, query.PageSize, cancellationToken);
        }
        else
        {
            files = await repository.GetPaginatedFilesAsync(query.PageNumber - 1, query.PageSize, cancellationToken);
        }

        var metadata = files.Select(file =>
        {
            var externalId = idEncoderService.Encode(file.Id)!;

            var accessLocation = locationService.GetFileAccessLocation(externalId);

            return file.ToMetadata(externalId, accessLocation);
        }).ToList();

        return Result<List<FileMetadata>>.Success(metadata);
    }

    public Task<Result<List<FileMetadata>>> ExecuteAsync(int pageNumber, int pageSize, string? category = null, CancellationToken cancellationToken = default)
        => ExecuteAsync(new GetPaginatedFilesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Category = category
        },
        cancellationToken);
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
    /// <param name="category">The category in which to search. If <c>null</c>, all files are paginated.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    /// <returns>A <see cref="Result{T}"/> containing a <see cref="List{T}"/> of <see cref="FileMetadata"/>, if successful.</returns>
    Task<Result<List<FileMetadata>>> ExecuteAsync(int pageNumber, int pageSize, string? category = null, CancellationToken cancellationToken = default);
}

internal class GetPaginatedFilesQuery : IQuery
{
    public required int PageNumber { get; init; }

    public required int PageSize { get; init; }

    public string? Category { get; init; }
}

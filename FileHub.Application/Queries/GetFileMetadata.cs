using FileHub.Application.Abstractions.Data.Repositories;
using FileHub.Application.Abstractions.Services;
using FileHub.Application.Extensions;
using FileHub.Core.Common;
using FileHub.Core.Files;
using Microsoft.Extensions.Logging;

namespace FileHub.Application.Queries;

internal sealed class GetFileMetadata(ILogger<GetFileMetadata> logger,
                                      IFileRepository repository,
                                      IFileLocationService locationService,
                                      IIdEncoderService<int> idEncoderService) : QueryHandler<GetFileMetadataQuery, FileMetadata>(logger), IGetFileMetadata
{
    protected override string Name => "Get File Metadata";

    protected override void Validate(ValidationState validationState, GetFileMetadataQuery query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.Id))
        {
            validationState.AddError(nameof(query.Id), "Must not be null, empty, or whitespace.");
        }
    }

    protected override async Task<Result<FileMetadata>> InternalExecuteAsync(GetFileMetadataQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Query '{Name}' started with external ID '{ExternalFileId}'.", Name, query.Id);

        var id = idEncoderService.Decode(query.Id);

        if (id == 0)
        {
            logger.LogError("Query '{Name}' could not decode the external ID '{ExternalFileId}'.", Name, query.Id);

            return Result<FileMetadata>.NotFound();
        }

        var file = await repository.GetByIdAsync(id, cancellationToken);

        if (file is null)
        {
            logger.LogError("Query '{Name}' could not find a file with ID '{FileId}'.", Name, id);

            return Result<FileMetadata>.NotFound();
        }

        var accessLocation = locationService.GetFileAccessLocation(query.Id);

        var metadata = file.ToMetadata(query.Id, accessLocation);

        return Result<FileMetadata>.Success(metadata);
    }

    public Task<Result<FileMetadata>> ExecuteAsync(string id, CancellationToken cancellationToken)
        => ExecuteAsync(new GetFileMetadataQuery
        {
            Id = id
        },
        cancellationToken);
}

/// <summary>
/// A query to get the metadata of a file.
/// </summary>
public interface IGetFileMetadata
{
    /// <summary>
    /// Execute the query to get the metadata of a file.
    /// </summary>
    /// <param name="id">The ID of the file.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    /// <returns>A <see cref="Result{T}"/> containing a <see cref="FileMetadata"/>, if successful.</returns>
    Task<Result<FileMetadata>> ExecuteAsync(string id, CancellationToken cancellationToken = default);
}

internal class GetFileMetadataQuery : IQuery
{
    public required string Id { get; init; }
}

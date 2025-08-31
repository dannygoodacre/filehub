using FileHub.Application.Abstractions.Data.Repositories;
using FileHub.Application.Abstractions.Services;
using FileHub.Core.Common;
using FileHub.Core.Files;
using Microsoft.Extensions.Logging;

namespace FileHub.Application.Queries;

internal sealed class GetFileContent(ILogger<GetFileContent> logger,
                                     IFileRepository repository,
                                     IFileStorageService storageService,
                                     IIdEncoderService<int> idEncoderService) : QueryHandler<GetFileContentQuery, FileContent>(logger), IGetFileContent
{
    protected override string Name => "Get File Content";

    protected override void Validate(ValidationState validationState, GetFileContentQuery query, CancellationToken cancellationToken)
    {
       if (string.IsNullOrWhiteSpace(query.Id))
       {
           validationState.AddError(nameof(query.Id), "Must not be null, empty, or whitespace.");
       }
    }

    protected override async Task<Result<FileContent>> InternalExecuteAsync(GetFileContentQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Query '{Name}' started with external ID '{ExternalFileId}'.", Name, query.Id);

        var id = idEncoderService.Decode(query.Id);

        if (id == 0)
        {
            logger.LogError("Query '{Name}' could not decode the external ID '{ExternalFileId}'.", Name, query.Id);

            return Result<FileContent>.NotFound();
        }

        var file = await repository.GetByIdAsync(id, cancellationToken);

        if (file is null)
        {
            logger.LogError("Query '{Name}' could not find a file with ID '{FileId}'.", Name, id);

            return Result<FileContent>.NotFound();
        }

        var contentResult = await storageService.OpenReadStreamAsync(file.StorageKey, cancellationToken);

        if (!contentResult.IsSuccess)
        {
            logger.LogError("Query '{Name}' could not read the file with ID '{FileId}' and Storage Key '{StorageKey}'.", Name, file.Id, file.StorageKey);

            return Result<FileContent>.Failed(contentResult.Error);
        }

        if (contentResult.Value is null)
        {
            logger.LogError("Query '{Name}' could not read the file with ID '{FileId}' and Storage Key '{StorageKey}'.", Name, file.Id, file.StorageKey);

            return Result<FileContent>.Failed("File content is empty.");
        }

        var content = new FileContent
        {
            ContentType = file.ContentType,
            Content = contentResult.Value
        };

        logger.LogInformation("Query '{Name}' completed for external ID '{ExternalFileId}', ID '{FileId}'.", Name, query.Id, file.Id);

        return Result<FileContent>.Success(content);
    }

    public Task<Result<FileContent>> ExecuteAsync(string id, CancellationToken cancellationToken = default)
        => ExecuteAsync(new GetFileContentQuery
        {
            Id = id
        },
        cancellationToken);
}

/// <summary>
/// A query to get the content of a file.
/// </summary>
public interface IGetFileContent
{
    /// <summary>
    /// Execute the query to get the content of a file.
    /// </summary>
    /// <param name="id">The unique ID of the file.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    /// <returns>A <see cref="Result{T}"/> containing a <see cref="FileContent"/>, if successful.</returns>
    Task<Result<FileContent>> ExecuteAsync(string id, CancellationToken cancellationToken = default);
}

internal class GetFileContentQuery : IQuery
{
    public required string Id { get; init; }
}

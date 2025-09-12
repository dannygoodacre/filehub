using FileHub.Application.Abstractions.Data;
using FileHub.Application.Abstractions.Data.Repositories;
using FileHub.Application.Abstractions.Services;
using FileHub.Application.Extensions;
using FileHub.Core.Common;
using FileHub.Core.Entities;
using Microsoft.Extensions.Logging;

namespace FileHub.Application.Commands;

internal sealed class AddFile(ILogger<AddFile> logger,
                              IFileRepository fileRepository,
                              ITagRepository tagRepository,
                              IFileStorageService storageService,
                              IApplicationContext context) : CommandHandler<AddFileCommand>(logger), IAddFile
{
    protected override string Name => "Add File";

    protected override void Validate(ValidationState validationState, AddFileCommand command, CancellationToken cancellationToken)
    {
        if (command.Content.Length == 0)
        {
            validationState.AddError(nameof(command.Content), "Must not be empty.");
        }

        if (!command.ContentType.IsValidMimeType())
        {
            validationState.AddError(nameof(command.ContentType), "Must be a valid MIME type.");
        }

        if (string.IsNullOrWhiteSpace(command.OriginalFileName))
        {
            validationState.AddError(nameof(command.OriginalFileName), "Must not be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            validationState.AddError(nameof(command.Name), "Must not be null or whitespace.");
        }

        if (command.UserId <= 0)
        {
            validationState.AddError(nameof(command.UserId), "Must be greater than 0.");
        }

        if (command.Tags is not null && command.Tags.Any(string.IsNullOrWhiteSpace))
        {
            validationState.AddError(nameof(command.Tags), "Must not be null or whitespace.");
        }
    }

    protected override async Task<Result> InternalExecuteAsync(AddFileCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Command '{Command}' started for File '{File}', Name '{Name}', User '{UserId}'.", Name, command.OriginalFileName, command.Name, command.UserId);

        var extension = Path.GetExtension(command.OriginalFileName);

        if (extension == string.Empty)
        {
            logger.LogError("Command '{Command}' could not determine the extension for File '{File}'.", Name, command.OriginalFileName);

            return Result.DomainError("Could not find file extension.");
        }

        var storageResult = await storageService.SaveAsync(command.Content, extension, cancellationToken);

        if (!storageResult.IsSuccess)
        {
            logger.LogError("Command '{Command}' could not save File '{File}'.", Name, command.OriginalFileName);

            return Result.InternalError("Could not save file.");
        }

        List<Tag> tags;

        var expectedChanges = 0;

        if (command.Tags is null || command.Tags.Count == 0)
        {
            tags = [];
        }
        else
        {
            tags = await tagRepository.GetManyForUpdateAsync(command.Tags, cancellationToken);

            // One for every link record linking the existing tag to the new file.
            expectedChanges += tags.Count;

            if (tags.Count < command.Tags.Count)
            {
                var existingTagNames = tags.Select(x => x.Name);

                var newTags = command.Tags
                    .Where(name => !existingTagNames.Contains(name))
                    .Select(name => new Tag { Name = name })
                    .ToList();

                tagRepository.AddRange(newTags);

                // One for each new tag and its corresponding link record.
                expectedChanges += 2 * newTags.Count;

                tags.AddRange(newTags);
            }
        }

        var file = new StoredFile
        {
            Name = command.Name,
            StorageKey = storageResult.Value,
            ContentType = command.ContentType,
            CreatedAt = DateTime.UtcNow,
            Tags = tags,
            UserId = command.UserId
        };

        fileRepository.Add(file);

        expectedChanges++;

        var actualChanges = await context.SaveChangesAsync();

        if (actualChanges != expectedChanges)
        {
            logger.LogError("Command '{Command}' wrote an unexpected number of entities to the database for File '{File}': expected '{Expected}', actual '{Actual}'.", Name, command.OriginalFileName, expectedChanges, actualChanges);
        }

        return Result.Success();
    }

    public Task<Result> ExecuteAsync(Stream content,
                                     string contentType,
                                     string originalFileName,
                                     string name,
                                     int userId,
                                     List<string>? tags,
                                     CancellationToken cancellationToken = default)
        => ExecuteAsync(new AddFileCommand
        {
            Content = content,
            ContentType = contentType,
            OriginalFileName = originalFileName,
            Name = name,
            UserId = userId,
            Tags = tags
        },
        cancellationToken);
}

/// <summary>
/// A command to create a new file entry and save its contents to storage.
/// </summary>
public interface IAddFile
{
    /// <summary>
    /// Execute the command to create a new file.
    /// </summary>
    /// <param name="content">A <see cref="Stream"/> of the content of the file.</param>
    /// <param name="contentType">The MIME type of the file.</param>
    /// <param name="originalFileName">The original name of the file, including its extension.</param>
    /// <param name="name">The name of the file.</param>
    /// <param name="userId">The ID of the user creating the file entry.</param>
    /// <param name="tags">A <see cref="List{T}"/> of tags associated with the file.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome of the operation.</returns>
    Task<Result> ExecuteAsync(Stream content,
                              string contentType,
                              string originalFileName,
                              string name,
                              int userId,
                              List<string>? tags,
                              CancellationToken cancellationToken = default);
}

internal class AddFileCommand : ICommand
{
    public required Stream Content { get; init; }

    public required string ContentType { get; init; }

    public required string OriginalFileName { get; init; }

    public required string Name { get; init; }

    public required int UserId { get; init; }

    public List<string>? Tags { get; init; }
}

using Api.Data.Entities;
using Api.Factories;
using Api.Models.Common;
using Api.Models.Files;
using Api.Repositories;

namespace Api.Services;

public class FileService(
    IStorageService storageService,
    ITagService tagService,
    IFileRepository fileRepository,
    ResponseFactory responseFactory,
    ILogger<FileService> logger
) : IFileService
{
    public async Task<Result> AddFileAsync(UploadRequest fileUploadRequest, User user)
    {
        var file = fileUploadRequest.File;
        if (!storageService.IsValidFile(file))
            return Result.Failure(Error.NoFileUploaded);

        List<Tag>? tags = null;
        if (fileUploadRequest.Tags is not null)
            tags = (await tagService.GetOrCreateTagsByNameAsync(fileUploadRequest.Tags)).ToList();

        var path = storageService.CreateFilePath(file);
        var storedFile = StoredFileFactory.Create(fileUploadRequest.Name, path, file.ContentType, user, tags);

        try
        {
            await storageService.SaveFileToPathAsync(file, storedFile.Path);
            logger.LogInformation("File {FileName} saved at {Path}", file.FileName, storedFile.Path);
        }
        catch (Exception)
        {
            logger.LogError("File {FileName} could not be saved at {Path}", file.FileName, storedFile.Path);
            return Result.Failure(Error.FileStorageError);
        }

        await fileRepository.AddAsync(storedFile);
        return Result.Success();
    }

    public async Task<Maybe<ContentResponse>> GetFileContentByIdAsync(int id)
    {
        if (id <= 0)
            return Maybe<ContentResponse>.None();

        var storedFile = await fileRepository.GetByIdAsync(id);
        if (storedFile is null)
            return Maybe<ContentResponse>.None();

        var contentResponse = new ContentResponse
        {
            ContentType = storedFile.ContentType,
            Data = await File.ReadAllBytesAsync(storedFile.Path),
        };

        return Maybe<ContentResponse>.Some(contentResponse);
    }

    public async Task<Maybe<MetaDataResponse>> GetFileMetaDataByIdAsync(int id)
    {
        if (id <= 0)
            return Maybe<MetaDataResponse>.None();

        var storedFile = await fileRepository.GetByIdAsync(id);
        if (storedFile is null)
            return Maybe<MetaDataResponse>.None();

        var metaDataResponse = responseFactory.CreateMetaDataResponse(storedFile);
        return Maybe.Some(metaDataResponse);
    }

    public async Task<Result<Maybe<List<MetaDataResponse>>>> GetAllFilesByTagAsync(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
            return Result<Maybe<List<MetaDataResponse>>>.Failure(Error.InvalidTagName);

        if (!await tagService.TagExistsByNameAsync(tagName))
            return Result<Maybe<List<MetaDataResponse>>>.Failure(Error.TagNotFound);

        var storedFiles = await fileRepository.GetAllByTagAsync(tagName);
        if (storedFiles.Count == 0)
            return Result.Success(Maybe<List<MetaDataResponse>>.None());

        var metaDataResponses = responseFactory.CreateMetaDataResponse(storedFiles);
        return Result.Success(Maybe.Some(metaDataResponses));
    }

    public async Task<Result<List<MetaDataResponse>>> GetPaginatedFilesAsync(int page, int pageSize)
    {
        if (page < 0 || pageSize <= 0)
            return Result<List<MetaDataResponse>>.Failure(Error.InvalidPage);

        var storedFiles = await fileRepository.GetPaginatedFilesAsync(page, pageSize);
        var metaDataResponses = responseFactory.CreateMetaDataResponse(storedFiles);
        return Result.Success(metaDataResponses);
    }
}

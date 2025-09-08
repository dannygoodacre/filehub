using FileHub.Application.Commands;
using FileHub.Application.Queries;
using FileHub.Web.Extensions;
using FileHub.Web.Models;
using FileHub.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileHub.Web.Controllers;

/// <summary>
/// Endpoints for handling file uploads and downloads.
/// </summary>
[Authorize]
[ApiController]
[Route("files")]
public class FileController(ICurrentUserService currentUserService,
                            IAddFile addFile,
                            IGetFileContent getFileContent,
                            IGetFileMetadata getFileMetadata,
                            IGetPaginatedFileMetadata getPaginatedFileMetadata,
                            IGetFilePageCount getFilePageCount) : ControllerBase
{
    /// <summary>
    /// Upload a file.
    /// </summary>
    [HttpPost("upload")]
    public async Task<IResult> UploadFileAsync([FromForm] UploadFileRequest request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();

        if (userId == 0)
        {
            return Results.Unauthorized();
        }

        var result = await addFile.ExecuteAsync(request.File.OpenReadStream(),
                                                request.File.ContentType,
                                                request.File.FileName,
                                                request.Name,
                                                userId,
                                                request.Tags,
                                                cancellationToken);

        return result.ToHttpResponse();
    }

    /// <summary>
    /// Fetch a file's content.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IResult> GetFileContentAsync(string id, CancellationToken cancellationToken = default)
    {
        var result = await getFileContent.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess
            ? Results.File(result.Value.Content, result.Value.ContentType)
            : result.ToHttpResponse();
    }

    /// <summary>
    /// Fetch a file's metadata.
    /// </summary>
    [HttpGet("{id}/metadata")]
    public async Task<IResult> GetFileMetadataAsync(string id, CancellationToken cancellationToken = default)
    {
        var result = await getFileMetadata.ExecuteAsync(id, cancellationToken);

        return result.ToHttpResponse();
    }
    
    /// <summary>
    /// Fetch the number of pages of files for the given page size.
    /// </summary>
    [HttpGet("pagecount")]
    public async Task<IResult> GetFilePageCountAsync([FromQuery] int pageSize, CancellationToken cancellationToken = default)
    {
        var result = await getFilePageCount.ExecuteAsync(pageSize, cancellationToken);

        return result.ToHttpResponse();
    }

    /// <summary>
    /// Fetch a paginated collection of file metadata.
    /// </summary>
    [HttpGet]
    public async Task<IResult> GetPaginatedFileMetadataAsync([FromQuery] int page = 1, [FromQuery] int count = 10, CancellationToken cancellationToken = default)
    {
        var result = await getPaginatedFileMetadata.ExecuteAsync(page, count, cancellationToken);

        return result.ToHttpResponse();
    }
}

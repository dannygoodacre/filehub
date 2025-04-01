using System.Net;
using Api.Data.Entities;
using Api.Models.Common;
using Api.Models.Files;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("files")]
public class FileController(UserManager<User> userManager, IFileService fileService, ILogger<FileController> logger) : ControllerBase
{
    /// <summary>
    /// Upload a file.
    /// </summary>
    [HttpPost("upload")]
    [SwaggerResponse(HttpStatusCode.OK, typeof(string), Description = Messages.FileUploaded)]
    [SwaggerResponse(HttpStatusCode.BadRequest, typeof(string), Description = Messages.NoFileUploaded)]
    [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(string), Description = Messages.InternalServerError)]
    public async Task<IActionResult> UploadFileAsync([FromForm] UploadRequest fileUploadRequest)
    {
        var user = await GetUserAsync();
        if (user is null)
            return Unauthorized(Messages.NotLoggedIn);
        
        logger.LogInformation("File {FileName} upload requested by {UserName}", 
            fileUploadRequest.File.FileName, user.UserName);
        
        var result = await fileService.AddFileAsync(fileUploadRequest, user);

        if (!result.IsSuccess)
        {
            logger.LogError("File {FileName} upload failed for {UserName} with error {Error}", 
                fileUploadRequest.File.FileName, user.UserName, result.Error);
            
            return CreateHttpErrorResponse(result.Error);
        }
        
        logger.LogInformation("File {FileName} uploaded successfully by {UserName}", 
            fileUploadRequest.File.FileName, user.UserName);
        
        return Ok(Messages.FileUploaded);
    }

    /// <summary>
    /// Fetch a file.
    /// </summary>
    [HttpGet("{id:int}")]
    [SwaggerResponse(HttpStatusCode.OK, typeof(ContentResponse))]
    [SwaggerResponse(HttpStatusCode.NotFound, typeof(string), Description = Messages.FileNotFound)]
    public async Task<IActionResult> GetFileAsync(int id)
    {
        var fileContent = await fileService.GetFileContentByIdAsync(id);

        return fileContent.HasValue
            ? File(fileContent.Value.Data, fileContent.Value.ContentType) 
            : NotFound(Messages.FileNotFound);
    }

    /// <summary>
    /// Fetch a file's metadata.
    /// </summary>
    [HttpGet("{id:int}/metadata")]
    [SwaggerResponse(HttpStatusCode.OK, typeof(MetaDataResponse))]
    [SwaggerResponse(HttpStatusCode.NotFound, typeof(string), Description = Messages.FileNotFound)]
    public async Task<IActionResult> GetFileMetadataAsync(int id)
    {
        var fileMetaData = await fileService.GetFileMetaDataByIdAsync(id);
        return fileMetaData.HasValue 
            ? Ok(fileMetaData.Value) 
            : NotFound(Messages.FileNotFound);
    }

    /// <summary>
    /// Fetch all files with a given tag.
    /// </summary>
    [HttpGet("tag/{tagName}")]
    [SwaggerResponse(HttpStatusCode.OK, typeof(List<MetaDataResponse>))]
    [SwaggerResponse(HttpStatusCode.BadRequest, typeof(string), Description = Messages.InvalidTagName)]
    [SwaggerResponse(HttpStatusCode.NotFound, typeof(string), Description = Messages.TagNotFound)]
    public async Task<IActionResult> GetAllFilesByTagAsync(string tagName)
    {
        var result = await fileService.GetAllFilesByTagAsync(tagName);
        if (!result.IsSuccess)
            return CreateHttpErrorResponse(result.Error);

        var fileMetaData = result.Content;
        return Ok(fileMetaData.HasValue ? fileMetaData.Value : []);
    }

    /// <summary>
    /// Fetch a subset of files using pagination.
    /// </summary>
    /// <param name="page">The page number to retrieve (zero-based).</param>
    /// <param name="size">The number of files per page.</param>
    [HttpGet]
    [SwaggerResponse(HttpStatusCode.OK, typeof(List<MetaDataResponse>))]
    [SwaggerResponse(HttpStatusCode.BadRequest, typeof(string), Description = Messages.InvalidPageRequested)]
    public async Task<IActionResult> GetPaginatedFilesAsync([FromQuery] int page, [FromQuery] int size)
    {
        var result = await fileService.GetPaginatedFilesAsync(page, size);
        return result.IsSuccess
            ? Ok(result.Content)
            : CreateHttpErrorResponse(result.Error);
    }

    private async Task<User?> GetUserAsync()
    {
        var user = User.Identity;
        if (user?.Name is null)
            return null;

        return await userManager.FindByNameAsync(user.Name);
    }

    /// <summary>
    /// Map internal error codes to HTTP responses.
    /// </summary>
    private ObjectResult CreateHttpErrorResponse(Error? error)
    {
        return error switch
        {
            Error.NoFileUploaded => BadRequest(Messages.NoFileUploaded),
            Error.InvalidTagName => BadRequest(Messages.InvalidTagName),
            Error.TagNotFound => NotFound(Messages.TagNotFound),
            Error.InvalidPage => BadRequest(Messages.InvalidPageRequested),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                Messages.InternalServerError
            ),
        };
    }
}

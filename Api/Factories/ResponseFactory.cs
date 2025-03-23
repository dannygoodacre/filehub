using Api.Data.Entities;
using Api.Models.Files;

namespace Api.Factories;

public class ResponseFactory
{
    private readonly string _baseUrl;

    /// <param name="httpContextAccessor">The HTTP context accessor used to build the base URL.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpContextAccessor"/> is null.</exception>
    public ResponseFactory(IHttpContextAccessor httpContextAccessor)
    {
        var request = httpContextAccessor.HttpContext?.Request;
        if (request is null)
            throw new ArgumentNullException(nameof(httpContextAccessor));

        _baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
    }

    /// <summary>
    /// Create a metadata response from a stored file entity.
    /// </summary>
    public MetaDataResponse CreateMetaDataResponse(StoredFile storedFile) =>
        new()
        {
            Name = storedFile.Name,
            Url = $"{_baseUrl}/files/{storedFile.Id}",
            ContentType = storedFile.ContentType,
            CreatedAt = storedFile.CreatedAt,
            Uploader = storedFile.Uploader.UserName!,
            Tags = storedFile.Tags.Select(tag => tag.Name).ToList(),
        };

    /// <summary>
    /// Create metadata responses from stored file entities.
    /// </summary>
    public List<MetaDataResponse> CreateMetaDataResponse(List<StoredFile> storedFiles) => 
        storedFiles.Select(CreateMetaDataResponse).ToList();
}

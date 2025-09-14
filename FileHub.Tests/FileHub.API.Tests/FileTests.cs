using System.Net.Http.Headers;
using System.Net.Http.Json;
using FileHub.Core.Entities;
using FileHub.Core.Files;
using FileHub.Web;
using NUnit.Framework;

namespace FileHub.API.Tests;

[TestFixture]
public class FileTests
{
    private FileHubWebApplicationFactory<Program> _factory = null!;

    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new FileHubWebApplicationFactory<Program>();

        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _factory.Dispose();
    }

    [Test]
    public async Task Post_UploadFileAsync()
    {
        // Arrange
        var content = new MultipartFormDataContent();

        var fileContent = new ByteArrayContent("Test file content"u8.ToArray());

        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");

        content.Add(fileContent, "file", "test.txt");

        content.Add(new StringContent("Test name"), "Name");

        content.Add(new StringContent("Category 1"), "Category");

        content.Add(new StringContent("Test tag 1"), "Tags");
        content.Add(new StringContent("Test tag 2"), "Tags");

        // Act
        var response = await _client.PostAsync("/files/upload", content);

        // Assert
        Assert.That(response.IsSuccessStatusCode);
    }

    [Test]
    public async Task Get_GetFileContentAsync()
    {
        // Arrange
        var id = _factory.ExternalFileIds[0];

        // Act
        var response = await _client.GetAsync($"/files/{id}");

        // Assert
        Assert.That(response.IsSuccessStatusCode);

        Assert.That(response.Content, Is.TypeOf<StreamContent>());

        var content = await response.Content.ReadAsStringAsync();

        Assert.That(content, Is.EqualTo("Test File Content"));

        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/plain"));
    }

    [Test]
    public async Task Get_GetFileMetadataAsync()
    {
        // Arrange
        var id = _factory.ExternalFileIds[0];

        var expectedStoredFile = _factory.StoredFiles[0];

        // Act
        var response = await _client.GetAsync($"/files/{id}/metadata");

        // Assert
        Assert.That(response.IsSuccessStatusCode);

        var metadata = await response.Content.ReadFromJsonAsync<FileMetadata>();

        AssertFileMetadata(expectedStoredFile, metadata);
    }

    [Test]
    public async Task Get_GetFilePageCountAsync()
    {
        // Arrange
        const int pageSize = 5;

        const int expectedPageCount = 3;

        // Act
        var response = await _client.GetAsync($"/files/pagecount?pageSize={pageSize}");

        // Assert
        Assert.That(response.IsSuccessStatusCode);

        var pageCount = await response.Content.ReadFromJsonAsync<int>();

        Assert.That(pageCount, Is.EqualTo(expectedPageCount));
    }

    [Test]
    public async Task Get_GetPaginatedFileMetadataAsync()
    {
        // Arrange
        const int pageNumber = 2;

        const int pageSize = 5;

        var expectedStoredFiles = _factory.StoredFiles[5..10];

        // Act
        var response = await _client.GetAsync($"/files?page={pageNumber}&count={pageSize}");

        // Assert
        Assert.That(response.IsSuccessStatusCode);

        var metadata = await response.Content.ReadFromJsonAsync<List<FileMetadata>>();

        Assert.That(metadata?.Count, Is.EqualTo(pageSize));

        for (var i = 0; i < pageSize; i++)
        {
            AssertFileMetadata(expectedStoredFiles[i], metadata?[i]);
        }
    }

    [Test]
    public async Task Get_GetPaginatedFileMetadataByCategoryAsync()
    {
        // Arrange
        const string category = "Category 2";

        const int pageNumber = 1;

        const int pageSize = 4;

        var expectedStoredFiles = _factory.StoredFiles[9..13];

        // Act
        var response = await _client.GetAsync($"files?page={pageNumber}&count={pageSize}&category={category}");

        // Assert
        Assert.That(response.IsSuccessStatusCode);

        var metadata = await response.Content.ReadFromJsonAsync<List<FileMetadata>>();

        Assert.That(metadata?.Count, Is.EqualTo(pageSize));

        for (var i = 0; i < pageSize; i++)
        {
            AssertFileMetadata(expectedStoredFiles[i], metadata?[i]);
        }
    }

    private static void AssertFileMetadata(StoredFile storedFile, FileMetadata? metadata)
    {
        var externalId = metadata?.Id;
        var accessLocation = metadata?.AccessLocation;

        Assert.That(externalId, Is.Not.Null);
        Assert.That(accessLocation, Is.Not.Null);

        Assert.That(accessLocation!.EndsWith(externalId!), Is.True);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(metadata?.Name, Is.EqualTo(storedFile.Name));
            Assert.That(metadata?.CreatedAt, Is.EqualTo(storedFile.CreatedAt));
            Assert.That(metadata?.Category, Is.EqualTo(storedFile.Category.Name));
            Assert.That(metadata?.Tags, Is.EquivalentTo(storedFile.Tags.Select(x => x.Name)));
        }
    }
}

using System.Net;
using System.Net.Http.Headers;
using FileHub.Web;
using NUnit.Framework;

namespace FileHub.API.Tests;

public class FileTests
{
    private FileHubWebApplicationFactory<Program> _factory = null!;

    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new FileHubWebApplicationFactory<Program>();
    }

    [SetUp]
    public void SetUp()
    {
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

        content.Add(new StringContent("Test tag 1"), "Tags");
        content.Add(new StringContent("Test tag 2"), "Tags");

        // Act
        var response = await _client.PostAsync("/files/upload", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}

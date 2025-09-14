using System.Net.Http.Json;
using FileHub.Web;
using NUnit.Framework;

namespace FileHub.API.Tests;

[TestFixture]
public class CategoryTests
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
    public async Task Post_AddCategoryAsync()
    {
        // Arrange
        var content = new MultipartFormDataContent();

        content.Add(new StringContent("Test New Category"), "Category");

        // Act
        var response = await _client.PostAsync("/categories/add", content);

        // Assert
        Assert.That(response.IsSuccessStatusCode);
    }

    [Test]
    public async Task Get_GetAllCategoriesAsync()
    {
        // Act
        var response = await _client.GetAsync("/categories");

        // Assert
        Assert.That(response.IsSuccessStatusCode);

        var categories = await response.Content.ReadFromJsonAsync<List<string>>();

        Assert.That(categories, Is.EquivalentTo(_factory.Categories.Select(x => x.Name)));
    }
}

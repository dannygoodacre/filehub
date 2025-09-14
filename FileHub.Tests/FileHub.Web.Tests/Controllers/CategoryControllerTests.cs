using FileHub.Application.Commands;
using FileHub.Application.Queries;
using FileHub.Core.Common;
using FileHub.Web.Controllers;
using FileHub.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace FileHub.Web.Tests.Controllers;

[TestFixture]
public class CategoryControllerTests : TestBase
{
    private string _requestCategoryName;

    private AddCategoryRequest _testAddCategoryRequest;

    private Result _testAddCategoryResult;

    private Result<List<string>> _testGetAllCategoriesResult;

    private List<string> _testCategoryNames;

    private CancellationToken _cancellationToken;

    private Mock<IAddCategory> _mockAddCategory;

    private Mock<IGetAllCategories> _mockGetAllCategories;

    private CategoryController _controller;

    [SetUp]
    public void SetUp()
    {
        _requestCategoryName = "Test Category";

        _testAddCategoryRequest = new AddCategoryRequest() { Category = _requestCategoryName };

        _testAddCategoryResult = Result.Success();

        _testCategoryNames = ["Test Category 1", "Test Category 2", "Test Category 3"];

        _testGetAllCategoriesResult = Result<List<string>>.Success(_testCategoryNames);

        _cancellationToken = CancellationToken.None;

        _mockAddCategory = new Mock<IAddCategory>(MockBehavior.Strict);

        _mockGetAllCategories = new Mock<IGetAllCategories>(MockBehavior.Strict);

        _controller = new CategoryController(_mockAddCategory.Object, _mockGetAllCategories.Object);
    }

    [Test]
    public async Task AddCategoryAsync_WhenAddCategoryReturnsDomainError_ShouldReturnBadRequest()
    {
        // Arrange
        _testAddCategoryResult = Result.DomainError("Test Domain Error");

        Setup_AddCategory_ExecuteAsync();

        // Act
        var result = await _controller.AddCategoryAsync(_testAddCategoryRequest, _cancellationToken);

        // Assert

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.TypeOf<BadRequest<string>>());

            var badRequestResult = result as BadRequest<string>;

            Assert.That(badRequestResult?.Value, Is.EqualTo(_testAddCategoryResult.Error));
        }
    }

    [Test]
    public async Task AddCategoryAsync_WhenAddCategoryIsSuccessful_ShouldReturnOk()
    {
        // Arrange
        Setup_AddCategory_ExecuteAsync();

        // Act
        var result = await _controller.AddCategoryAsync(_testAddCategoryRequest, _cancellationToken);

        // Assert
        Assert.That(result, Is.EqualTo(Results.Ok()));
    }

    [Test]
    public async Task GetAllCategoriesAsync_WhenGetAllCategoriesFails_ShouldReturnInternalServerError()
    {
        // Arrange
        _testGetAllCategoriesResult = Result<List<string>>.InternalError("Test Internal Error");

        Setup_GetAllCategories_ExecuteAsync();

        // Act
        var result = await _controller.GetAllCategoriesAsync(_cancellationToken);

        // Assert
        Assert.That(result, Is.EqualTo(Results.InternalServerError()));
    }

    [Test]
    public async Task GetAllCategoriesAsync_WhenGetAllCategoriesIsSuccessful_ShouldReturnOk()
    {
        // Arrange
        Setup_GetAllCategories_ExecuteAsync();

        // Act
        var result = await _controller.GetAllCategoriesAsync(_cancellationToken);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.TypeOf<Ok<List<string>>>());

            var okResult = result as Ok<List<string>>;

            Assert.That(okResult?.Value, Is.EqualTo(_testGetAllCategoriesResult.Value));
        }
    }

    private void Setup_AddCategory_ExecuteAsync(int times = 1)
    {
        _mockAddCategory
            .Setup(x => x.ExecuteAsync(
                It.Is<string>(y => y == _requestCategoryName),
                It.Is<CancellationToken>(y => y == _cancellationToken)))
            .ReturnsAsync(_testAddCategoryResult)
            .Verifiable(Times.Exactly(times));
    }

    private void Setup_GetAllCategories_ExecuteAsync(int times = 1)
    {
        _mockGetAllCategories
            .Setup(x => x.ExecuteAsync(
                It.Is<CancellationToken>(y => y == _cancellationToken)))
            .ReturnsAsync(_testGetAllCategoriesResult)
            .Verifiable(Times.Exactly(times));
    }
}

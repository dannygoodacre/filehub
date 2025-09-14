using FileHub.Application.Abstractions.Data.Repositories;
using FileHub.Application.Queries;
using FileHub.Core.Common;
using FileHub.Core.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace FileHub.Application.Tests.Queries;

[TestFixture]
public class GetAllCategoriesTests : TestBase
{
    private const string Name = "Get All Categories";

    private Mock<ILogger<GetAllCategories>> _mockLogger;

    private Mock<ICategoryRepository> _mockCategoryRepository;

    private GetAllCategories _query;

    private CancellationToken _testCancellationToken;

    private List<Category> _categories;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<GetAllCategories>>(MockBehavior.Strict);

        _mockCategoryRepository = new Mock<ICategoryRepository>(MockBehavior.Strict);

        _query = new GetAllCategories(_mockLogger.Object, _mockCategoryRepository.Object);

        _testCancellationToken = CancellationToken.None;

        _categories =
        [
            new Category
            {
                Id = 123,
                Name = "Test Category 1"
            },

            new Category
            {
                Id = 456,
                Name = "Test Category 2"
            }
        ];
    }

    [Test]
    public async Task ExecuteAsync_WhenNoCategories_ShouldReturnSuccess()
    {
        // Arrange
        Setup_Logger_Starting();

        _categories = [];

        Setup_CategoryRepository_GetAllAsync();

        // Act
        var result = await Act();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            AssertSuccess(result);

            Assert.That(result.Value, Is.Empty);
        }
    }

    [Test]
    public async Task ExecuteAsync_WhenCategories_ShouldReturnSuccess()
    {
        // Arrange
        Setup_Logger_Starting();

        Setup_CategoryRepository_GetAllAsync();

        // Act
        var result = await Act();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            AssertSuccess(result);

            Assert.That(result.Value, Is.EquivalentTo(_categories.Select(x => x.Name)));
        }
    }

    private async Task<Result<List<string>>> Act() => await _query.ExecuteAsync(_testCancellationToken);

    private void Setup_Logger_Starting()
    {
        _mockLogger.Setup(LogLevel.Information, $"Query '{Name}' started.");
    }

    private void Setup_CategoryRepository_GetAllAsync()
    {
        _mockCategoryRepository
            .Setup(x => x.GetAllAsync(
                It.Is<CancellationToken>(y => y == _testCancellationToken)))
            .ReturnsAsync(_categories)
            .Verifiable(Times.Once);
    }
}

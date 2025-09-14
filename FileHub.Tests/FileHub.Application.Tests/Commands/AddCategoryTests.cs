using FileHub.Application.Abstractions.Data;
using FileHub.Application.Abstractions.Data.Repositories;
using FileHub.Application.Commands;
using FileHub.Core.Common;
using FileHub.Core.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace FileHub.Application.Tests.Commands;

[TestFixture]
public class AddCategoryTests : TestBase
{
    private const string Name = "Add Category";

    private Mock<ILogger<AddCategory>> _mockLogger;

    private Mock<ICategoryRepository> _mockCategoryRepository;

    private Mock<IApplicationContext> _mockApplicationContext;

    private AddCategory _command;

    private string _requestCategoryName;

    private int _testActualChanges;

    private bool _testCategoryExists;

    private CancellationToken _cancellationToken;

    [SetUp]
    public void SetUp()
    {
        _requestCategoryName = "Test Category Name";

        _testActualChanges = 1;

        _testCategoryExists = false;

        _cancellationToken = CancellationToken.None;

        _mockLogger = new Mock<ILogger<AddCategory>>(MockBehavior.Strict);

        _mockCategoryRepository = new Mock<ICategoryRepository>(MockBehavior.Strict);

        _mockApplicationContext = new Mock<IApplicationContext>(MockBehavior.Strict);

        _command = new AddCategory(_mockLogger.Object,
                                   _mockCategoryRepository.Object,
                                   _mockApplicationContext.Object);
    }

    [TestCase(null!)]
    [TestCase("")]
    [TestCase(" ")]
    public async Task ExecuteAsync_WhenCategoryInvalid_ShouldReturnInvalid(string category)
    {
        // Arrange
        _requestCategoryName = category;

        _mockLogger.Setup(LogLevel.Error, $"Command '{Name}' failed validation: CategoryName:{Environment.NewLine}  - Must not be null, empty, or whitespace.");

        // Act
        var result = await Act();

        // Assert
        AssertInvalid(result);
    }

    [Test]
    public async Task ExecuteAsync_WhenCategoryExists_ShouldReturnDomainError()
    {
        // Arrange
        Setup_Logger_Starting();

        _testCategoryExists = true;

        Setup_CategoryRepository_ExistsAsync();

        _mockLogger.Setup(LogLevel.Error, $"Command '{Name}' did not create the category '{_requestCategoryName}' as it already exists.");

        // Act
        var result = await Act();

        // Assert
        AssertDomainError(result, "Category already exists.");
    }

    [Test]
    public async Task ExecuteAsync_WhenUnexpectedNumberOfChanges_ShouldReturnSucccess()
    {
        // Arrange
        Setup_Logger_Starting();

        Setup_CategoryRepository_ExistsAsync();

        Setup_CategoryRepository_Add();

        _testActualChanges = 5;

        Setup_ApplicationContext_SaveChangesAsync();

        _mockLogger.Setup(LogLevel.Error, $"Command '{Name}' wrote an unexpected number of entities to the database for Category '{_requestCategoryName}': expected '1', actual '{_testActualChanges}'.");

        // Act
        var result = await Act();

        // Assert
        AssertSuccess(result);
    }

    private async Task<Result> Act() => await _command.ExecuteAsync(_requestCategoryName, _cancellationToken);

    private void Setup_Logger_Starting(int times = 1)
    {
        _mockLogger.Setup(LogLevel.Information, $"Command '{Name}' started with category '{_requestCategoryName}'.");
    }

    private void Setup_CategoryRepository_ExistsAsync(int times = 1)
    {
        _mockCategoryRepository
            .Setup(x => x.ExistsAsync(
                It.Is<string>(y => y == _requestCategoryName),
                It.Is<CancellationToken>(y => y == _cancellationToken)))
            .ReturnsAsync(_testCategoryExists)
            .Verifiable(Times.Exactly(times));
    }

    private void Setup_CategoryRepository_Add(int times = 1)
    {
        _mockCategoryRepository
            .Setup(x => x.Add(
                It.Is<Category>(y => y.Name == _requestCategoryName)))
            .Verifiable(Times.Exactly(times));

    }

    private void Setup_ApplicationContext_SaveChangesAsync(int times = 1)
    {
        _mockApplicationContext
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(_testActualChanges))
            .Verifiable(Times.Exactly(times));
    }
}

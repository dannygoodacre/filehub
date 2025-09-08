using FileHub.Application.Abstractions.Data.Repositories;
using FileHub.Application.Queries;
using FileHub.Core.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace FileHub.Application.Tests.Queries;

[TestFixture]
public class GetFilePageCountTests : TestBase
{
    private const string Name = "Get File Page Count";

    private Mock<ILogger<GetFilePageCount>> _mockLogger = null!;

    private Mock<IFileRepository> _mockRepository = null!;

    private GetFilePageCount _query;

    private int _requestPageSize;

    private CancellationToken _testCancellationToken;

    private int _testFileCount;

    private int _testPageCount;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<GetFilePageCount>>(MockBehavior.Strict);

        _mockRepository = new Mock<IFileRepository>(MockBehavior.Strict);

        _query = new GetFilePageCount(_mockLogger.Object, _mockRepository.Object);

        _testCancellationToken = CancellationToken.None;

        _requestPageSize = 10;

        _testFileCount = 36;

        _testPageCount = 4;
    }

    [TestCase(0)]
    [TestCase(-1)]
    public async Task ExecuteAsync_WhenPageSizeInvalid_ShouldReturnInvalid(int pageSize)
    {
        // Arrange
        _requestPageSize = pageSize;

        _mockLogger.Setup(LogLevel.Error, $"Query '{Name}' failed validation: PageSize:{Environment.NewLine}  - Must be greater than 0.");

        // Act
        var result = await Act();

        // Assert
        AssertInvalid(result);
    }

    [Test]
    public async Task ExecuteAsync_WhenSuccess_ShouldReturnPageCount()
    {
        // Arrange
        _mockLogger.Setup(LogLevel.Information, $"Query '{Name}' started with page size '{_requestPageSize}'.");

        _mockRepository
            .Setup(x => x.GetFilesCountAsync(
                It.Is<CancellationToken>(y => y == _testCancellationToken)
            ))
            .ReturnsAsync(_testFileCount)
            .Verifiable(Times.Once);

        _mockLogger.Setup(LogLevel.Information, $"Query '{Name}' completed with total file count '{_testFileCount}', total page count '{_testPageCount}', page size '{_requestPageSize}'.");

        // Act
        var result = await Act();

        // Assert

        using (Assert.EnterMultipleScope())
        {
            AssertSuccess(result);

            Assert.That(result?.Value, Is.EqualTo(_testPageCount));
        }
    }

    private async Task<Result<int>> Act() => await _query.ExecuteAsync(_requestPageSize, _testCancellationToken);
}

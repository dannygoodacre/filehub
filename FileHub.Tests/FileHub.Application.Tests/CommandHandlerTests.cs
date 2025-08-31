using FileHub.Core.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace FileHub.Application.Tests;

[TestFixture]
public class CommandHandlerTests : TestBase
{
    public class TestCommand : ICommand;

    private const string TestName = "Test Command Handler";

    private const string TestProperty = "Test Property";

    private const string TestError = "Test Error";

    private const string TestExceptionMessage = "Test Exception Message";

    private readonly CancellationToken _testCancellationToken = CancellationToken.None;

    private readonly TestCommand _testCommand = new();

    private static Action<ValidationState, TestCommand, CancellationToken> _validate = (_, _, _) => {};

    private static Func<TestCommand, CancellationToken, Task<Result>> _internalExecuteAsync = (_, _) => Task.FromResult(new Result());

    private Mock<ILogger<TestCommandHandler>> _loggerMock = null!;

    public class TestCommandHandler(ILogger logger) : CommandHandler<TestCommand>(logger)
    {
        protected override string Name => TestName;

        protected override void Validate(ValidationState validationState, TestCommand command, CancellationToken cancellationToken)
            => _validate(validationState, command, cancellationToken);

        protected override Task<Result> InternalExecuteAsync(TestCommand command, CancellationToken cancellationToken)
            => _internalExecuteAsync(command, cancellationToken);

        public Task<Result> TestExecuteAsync(TestCommand command, CancellationToken cancellationToken) => ExecuteAsync(command, cancellationToken);
    }

    [Test]
    public async Task ExecuteAsync_WhenValidationFails_ShouldReturnInvalid()
    {
        // Arrange
        _loggerMock = new Mock<ILogger<TestCommandHandler>>(MockBehavior.Strict);

        _loggerMock.Setup(LogLevel.Error, $"Command '{TestName}' failed validation: {TestProperty}:{Environment.NewLine}  - {TestError}");

        _validate = (validationState, _, _)
            => validationState.AddError(TestProperty, TestError);

        var handler = new TestCommandHandler(_loggerMock.Object);

        // Act
        var result = await handler.TestExecuteAsync(_testCommand, _testCancellationToken);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo(Status.Invalid));
        }
    }

    [Test]
    public async Task ExecuteAsync_WhenCancelled_ShouldReturnCancelled()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();

        _loggerMock = new Mock<ILogger<TestCommandHandler>>(MockBehavior.Strict);

        _loggerMock.Setup(LogLevel.Information, $"Command '{TestName}' was cancelled before execution.");

        var handler = new TestCommandHandler(_loggerMock.Object);

        await cancellationTokenSource.CancelAsync();

        // Act
        var result = await handler.TestExecuteAsync(_testCommand, cancellationTokenSource.Token);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo(Status.Cancelled));
        }
    }

    [Test]
    public async Task ExecuteAsync_WhenCancelledDuring_ShouldReturnCancelled()
    {
        // Arrange
        _loggerMock = new Mock<ILogger<TestCommandHandler>>(MockBehavior.Strict);

        _loggerMock.Setup(LogLevel.Information, $"Command '{TestName}' was cancelled during execution.");

        _internalExecuteAsync = (_, _) => throw new OperationCanceledException();

        var handler = new TestCommandHandler(_loggerMock.Object);

        // Act
        var result = await handler.TestExecuteAsync(_testCommand, _testCancellationToken);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo(Status.Cancelled));
        }
    }

    [Test]
    public async Task ExecuteAsync_WhenExceptionOccurs_ShouldReturnInternalError()
    {
        // Arrange
        var exception = new ApplicationException(TestExceptionMessage);

        _loggerMock = new Mock<ILogger<TestCommandHandler>>(MockBehavior.Strict);

        _loggerMock.Setup(
            LogLevel.Critical,
            $"Command '{TestName}' failed with exception: {TestExceptionMessage}",
            exception: exception);

        _internalExecuteAsync = (_, _) => throw exception;

        var handler = new TestCommandHandler(_loggerMock.Object);

        // Act
        var result = await handler.TestExecuteAsync(_testCommand, _testCancellationToken);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo(Status.InternalError));
        }
    }
}

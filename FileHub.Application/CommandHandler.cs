using FileHub.Core.Common;
using Microsoft.Extensions.Logging;

namespace FileHub.Application;

public abstract class CommandHandler<TCommand>(ILogger logger) where TCommand : ICommand
{
    protected abstract string Name { get; }

    /// <summary>
    /// Validate the command before execution.
    /// </summary>
    /// <param name="validationState">A <see cref="ValidationState"/> to populate with the operation's outcome.</param>
    /// <param name="command">The command request to validate.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    protected abstract void Validate(ValidationState validationState, TCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// The internal command logic.
    /// </summary>
    /// <param name="command">The valid command request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome of the operation.</returns>
    protected abstract Task<Result> InternalExecuteAsync(TCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Run the command by validating first and, if successful, execute the internal logic.
    /// </summary>
    /// <param name="command">The command request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome of the operation.</returns>
    protected async Task<Result> ExecuteAsync(TCommand command, CancellationToken cancellationToken)
    {
        var validationState = new ValidationState();

        Validate(validationState, command, cancellationToken);

        if (validationState.HasErrors)
        {
            logger.LogError("Command '{Command}' failed validation: {ValidationState}", Name, validationState);

            return Result.Invalid(validationState);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Command '{Command}' was cancelled before execution.", Name);

            return Result.Cancelled();
        }

        try
        {
            return await InternalExecuteAsync(command, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Command '{Command}' was cancelled during execution.", Name);

            return Result.Cancelled();
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Command '{Command}' failed with exception: {Exception}", Name, e.Message);

            return Result.InternalError(e.Message);
        }
    }
}

/// <summary>
/// A command request.
/// </summary>
public interface ICommand;

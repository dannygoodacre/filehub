using FileHub.Core.Common;
using Microsoft.Extensions.Logging;

namespace FileHub.Application;

public abstract class QueryHandler<TQuery, TResult>(ILogger logger) where TQuery : IQuery
{
    protected abstract string Name { get; }

    /// <summary>
    /// Validate the query before execution.
    /// </summary>
    /// <param name="validationState">A <see cref="ValidationState"/> to populate with the operation's outcome.</param>
    /// <param name="query">The query request to validate.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    protected abstract void Validate(ValidationState validationState, TQuery query, CancellationToken cancellationToken);

    /// <summary>
    /// The internal query logic.
    /// </summary>
    /// <param name="query">The valid query request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    /// <returns>A <see cref="Result{T}"/> indicating the outcome of the operation.</returns>
    protected abstract Task<Result<TResult>> InternalExecuteAsync(TQuery query, CancellationToken cancellationToken);

    /// <summary>
    /// Run the query by validating first and, if successful, execute the internal logic.
    /// </summary>
    /// <param name="query">The query request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
    /// <returns>A <see cref="Result{T}"/> indicating the outcome of the operation.</returns>
    protected async Task<Result<TResult>> ExecuteAsync(TQuery query, CancellationToken cancellationToken)
    {
        var validationState = new ValidationState();

        Validate(validationState, query, cancellationToken);

        if (validationState.HasErrors)
        {
            logger.LogError("Query '{Query}' failed validation: {ValidationState}", Name, validationState);

            return Result<TResult>.Invalid(validationState);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Query '{Query}' was cancelled before execution.", Name);

            return Result<TResult>.Cancelled();
        }

        try
        {
            return await InternalExecuteAsync(query, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Query '{Query}' was cancelled during execution.", Name);

            return Result<TResult>.Cancelled();
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Query '{Query}' failed with exception: {Exception}", Name, e.Message);

            return Result<TResult>.InternalError(e.Message);
        }
    }
}

/// <summary>
/// A query request.
/// </summary>
public interface IQuery;

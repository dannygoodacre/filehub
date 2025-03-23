using Api.Models.Common;

namespace Api.Middleware;

/// <summary>
/// Hide unhandled exceptions that occur during the processing of an HTTP request.
/// </summary>
/// <param name="next">The next middleware component in the pipeline.</param>
/// <param name="logger">The logger for logging error information.</param>
public class ExceptionHandler(RequestDelegate next, ILogger<ExceptionHandler> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            logger.LogError(e, Messages.UnhandledException);
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync(Messages.InternalServerError);
        }
    }
}

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MyFundex.Api.Infrastructure;

public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> log)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested) { }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
                throw;
            var (status, title) = exception switch
            {
                DbUpdateConcurrencyException => (409, "The record changed. Refresh and retry."),
                DbUpdateException
                {
                    InnerException: PostgresException
                    {
                        SqlState: PostgresErrorCodes.UniqueViolation
                    }
                } => (409, "A record with this identifier already exists."),
                ArgumentException => (400, "Invalid request."),
                JsonException => (400, "Invalid JSON payload."),
                HttpRequestException => (
                    502,
                    "Provider request failed; check the operation status before retrying."
                ),
                _ => (500, "Unexpected server error."),
            };
            log.LogError(exception, "Request failed {CorrelationId}", context.TraceIdentifier);
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(
                new
                {
                    title,
                    status,
                    correlationId = context.TraceIdentifier,
                }
            );
        }
    }
}

using System.Text.Json;
using OrderApi.Services.Exceptions;

namespace OrderApi.Middleware;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ApiException ex)
        {
            await WriteProblemDetailsAsync(context, ex.StatusCode, ex.Message, ex.ErrorCode, ex.Details);
        }
        catch (Exception ex)
        {
            // Log unexpected errors (structured)
            var logger = context.RequestServices.GetRequiredService<ILogger<ExceptionHandlingMiddleware>>();
            logger.LogError(ex, "Unhandled exception");

            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                "ServerError",
                null);
        }
    }

    private static async Task WriteProblemDetailsAsync(
        HttpContext context,
        int statusCode,
        string message,
        string errorCode,
        IDictionary<string, object?>? details)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var payload = new Dictionary<string, object?>
        {
            ["type"] = "about:blank",
            ["title"] = message,
            ["status"] = statusCode,
            ["traceId"] = context.TraceIdentifier,
            ["errorCode"] = errorCode
        };

        if (details is not null)
        {
            foreach (var kv in details)
                payload[kv.Key] = kv.Value;
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }
}

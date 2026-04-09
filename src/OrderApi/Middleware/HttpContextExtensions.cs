namespace OrderApi.Middleware;

public static class HttpContextExtensions
{
    public static string GetCorrelationId(this HttpContext context)
    {
        return context.Items.TryGetValue(CorrelationIdMiddleware.HeaderName, out var v) && v is string s && !string.IsNullOrWhiteSpace(s)
            ? s
            : context.TraceIdentifier;
    }
}

using System.Diagnostics;

namespace Shared.Observability;

public static class Telemetry
{
    // One consistent activity source name across API + Worker
    public const string ActivitySourceName = "EventDrivenOrderProcessing";
    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    // W3C trace context keys
    public const string TraceParent = "traceparent";
    public const string TraceState = "tracestate";
}

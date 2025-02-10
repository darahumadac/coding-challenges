using Serilog;
using Serilog.Context;

namespace middleware;

public class RequestContextMiddleware
{
    private readonly RequestDelegate _next;
    public RequestContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public Task InvokeAsync(HttpContext context)
    {
        using (LogContext.PushProperty("CorrelationId", context.TraceIdentifier))
        {
            //adds CorrelationId to all context
            return _next(context);
        }
    }
}
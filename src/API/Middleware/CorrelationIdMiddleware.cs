using Serilog.Context;

namespace API.Middleware;

public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // Tạo và quản lý Correlation ID cho mỗi yêu cầu
    public async Task InvokeAsync(HttpContext context)
    {
        // Lấy Correlation ID từ header hoặc tạo mới nếu không có
        var correlationId =
            context.Request.Headers.TryGetValue(HeaderName, out var incoming) && !string.IsNullOrWhiteSpace(incoming)
                ? incoming.ToString()
                : Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        // Thêm Correlation ID vào Log Context
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
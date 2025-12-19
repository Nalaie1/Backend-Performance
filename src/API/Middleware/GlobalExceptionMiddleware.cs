using Serilog;

namespace API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // Ném ngoại lệ không xử lý và trả về phản hồi lỗi chuẩn
    public async Task InvokeAsync(HttpContext context)
    {
        // Error handling
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unhandled exception");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var result = new
            {
                Status = 500,
                Message = "Internal Server Error",
                CorrelationId = context.Items["CorrelationId"]
            };
            await context.Response.WriteAsJsonAsync(result);
        }
    }
}
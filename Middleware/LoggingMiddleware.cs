namespace CryptoTracker.Middleware;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var method = context.Request.Method;
        var path = context.Request.Path;
        
        _logger.LogInformation("[{StartTime}] {Method} {Path} başlatıldı", 
            startTime.ToString("HH:mm:ss"), method, path);

        await _next(context);

        var duration = DateTime.UtcNow - startTime;
        _logger.LogInformation("[{StartTime}] {Method} {Path} tamamlandı ({Duration}ms)", 
            startTime.ToString("HH:mm:ss"), method, path, duration.TotalMilliseconds);
    }
}
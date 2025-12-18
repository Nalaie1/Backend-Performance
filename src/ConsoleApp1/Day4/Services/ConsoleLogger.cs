using ConsoleApp1.Day4.Interfaces;

namespace ConsoleApp1.Day4.Services;

/// <summary>
/// Logger ghi log ra console.
/// </summary>
public class ConsoleLogger : ILogger
{
    private readonly object _lockObject = new();
    
    public void LogInfo(string message)
    {
        lock (_lockObject)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}");
            Console.ResetColor();
        }
    }
    
    public void LogWarning(string message)
    {
        lock (_lockObject)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[WARN] {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}");
            Console.ResetColor();
        }
    }
    
    public void LogError(string message, Exception? exception = null)
    {
        lock (_lockObject)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}");
            if (exception != null)
            {
                Console.WriteLine($"        Exception: {exception.GetType().Name}");
                Console.WriteLine($"        Message: {exception.Message}");
                if (exception.StackTrace != null)
                {
                    Console.WriteLine($"        StackTrace: {exception.StackTrace}");
                }
            }
            Console.ResetColor();
        }
    }
}
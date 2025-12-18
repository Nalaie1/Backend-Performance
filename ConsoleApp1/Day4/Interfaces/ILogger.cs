namespace ConsoleApp1.Day4.Interfaces;

/// <summary>
/// Interface chuyên biệt cho Logging.
/// </summary>
public interface ILogger
{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message, Exception? exception = null);
}
namespace ConsoleApp1.Day4.Interfaces;

/// <summary>
/// Interface cho đọc file.
/// </summary>
public interface IFileReader
{
    IAsyncEnumerable<string> ReadLinesAsync(string filePath, CancellationToken cancellationToken = default);
    IEnumerable<string> ReadLines(string filePath);
}
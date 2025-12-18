using System.Text;
using ConsoleApp1.Day4.Interfaces;

namespace ConsoleApp1.Day4.Services;

/// <summary>
/// Cài đặt IFileReader để đọc file.
/// Single Responsibility.
/// </summary>
public class FileReader : IFileReader
{
    private const int BufferSize = 1024 * 1024; // 1MB
    
    public async IAsyncEnumerable<string> ReadLinesAsync(
        string filePath,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }
        
        await using var fileStream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            BufferSize,
            FileOptions.SequentialScan | FileOptions.Asynchronous);
        
        using var reader = new StreamReader(fileStream, Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false, bufferSize: BufferSize);
        
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return line;
        }
    }
    
    public IEnumerable<string> ReadLines(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }
        
        using var reader = new StreamReader(filePath, Encoding.UTF8, 
            detectEncodingFromByteOrderMarks: false, bufferSize: BufferSize);
        
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            yield return line;
        }
    }
}
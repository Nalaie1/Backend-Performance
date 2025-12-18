using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace ConsoleApp1.Day1_2;

public class WordCounter
{
    public Dictionary<string, int> CountWordsSequential(string filePath)
    {
        var wordCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var regex = new Regex(@"\b\w+\b", RegexOptions.Compiled);
    
        using var reader = new StreamReader(filePath);
        string? line;
    
        while ((line = reader.ReadLine()) != null)
        {
            foreach (Match word in regex.Matches(line))
            {
                var key = word.Value;
                wordCount.TryGetValue(key, out var count);
                wordCount[key] = count + 1;
            }
        }
    
        return wordCount;
    }

    public Dictionary<string, int> CountWordsParallel(string filePath)
    {
        var wordCount = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var regex = new Regex(@"\b\w+\b", RegexOptions.Compiled);

        Parallel.ForEach(
            File.ReadLines(filePath, Encoding.UTF8),
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            line =>
            {
                foreach (Match word in regex.Matches(line))
                {
                    wordCount.AddOrUpdate(word.Value, 1, (_, c) => c + 1);
                }
            });

        return new Dictionary<string, int>(wordCount);
    }
}

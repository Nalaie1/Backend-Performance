using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using ConsoleApp1.Day4.Interfaces;

namespace ConsoleApp1.Day4.Services;

public class CaseSensitiveWordCounter : IWordCounter
{
    private static readonly Regex WordPattern = new(@"\b\w+\b", RegexOptions.Compiled);
    
    public Dictionary<string, int> CountWords(IEnumerable<string> lines)
    {
        var wordCount = new Dictionary<string, int>();
        
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            
            var matches = WordPattern.Matches(line);
            foreach (Match match in matches)
            {
                var word = match.Value; // Giữ nguyên case
                wordCount.TryGetValue(word, out var count);
                wordCount[word] = count + 1;
            }
        }
        
        return wordCount;
    }
    
    public Dictionary<string, int> CountWordsParallel(IEnumerable<string> lines)
    {
        var wordCount = new ConcurrentDictionary<string, int>();
        
        Parallel.ForEach(lines, new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        }, line =>
        {
            if (string.IsNullOrWhiteSpace(line)) return;
            
            var matches = WordPattern.Matches(line);
            foreach (Match match in matches)
            {
                var word = match.Value;
                wordCount.AddOrUpdate(word, 1, (_, count) => count + 1);
            }
        });
        
        return wordCount.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}
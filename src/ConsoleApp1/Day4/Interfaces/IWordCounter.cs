namespace ConsoleApp1.Day4.Interfaces;

/// <summary>
/// Interface cho đếm từ.
/// </summary>
public interface IWordCounter
{
    Dictionary<string,int> CountWords(IEnumerable<string> lines);
    Dictionary<string, int> CountWordsParallel(IEnumerable<string> lines);
}
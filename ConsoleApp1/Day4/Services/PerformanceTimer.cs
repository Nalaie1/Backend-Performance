using System.Diagnostics;
using ConsoleApp1.Day4.Interfaces;

namespace ConsoleApp1.Day4.Services;

public class PerformanceTimer : IPerformanceTimer
{
    public TimeSpan Measure(Action action)
    {
        var sw = Stopwatch.StartNew();
        action();
        sw.Stop();
        return sw.Elapsed;
    }

    public T Measure<T>(Func<T> action, out TimeSpan elapsed)
    {
        var sw = Stopwatch.StartNew();
        var result = action();
        sw.Stop();
        elapsed = sw.Elapsed;
        return result;
    }

    public async Task<TimeSpan> MeasureAsync(Func<Task> action)
    {
        var sw = Stopwatch.StartNew();
        await action();
        sw.Stop();
        return sw.Elapsed;
    }

    public async Task<(T Result, TimeSpan Elapsed)> MeasureAsync<T>(Func<Task<T>> action)
    {
        var sw = Stopwatch.StartNew();
        var result = await action();
        sw.Stop();
        return (result, sw.Elapsed);
    }
}
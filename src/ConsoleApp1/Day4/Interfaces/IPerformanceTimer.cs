using System.Diagnostics;

namespace ConsoleApp1.Day4.Interfaces;

/// <summary>
/// Interface cho việc đo thời gian thực thi.
/// </summary>
public interface IPerformanceTimer
{
    TimeSpan Measure(Action action);

    T Measure<T>(Func<T> action, out TimeSpan elapsed);

    Task<TimeSpan> MeasureAsync(Func<Task> action);

    Task<(T Result, TimeSpan Elapsed)> MeasureAsync<T>(Func<Task<T>> action);
}
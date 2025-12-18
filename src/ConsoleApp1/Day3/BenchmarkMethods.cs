using System.Diagnostics;
using System.Text;
using ConsoleApp1.Day4.Interfaces;
using ConsoleApp1.Day4.Services;

namespace ConsoleApp1.Day3;

public class BenchmarkMethods
{
    private const int RecordCount = 1_000_000;
    private readonly IPerformanceTimer _timer;
    public BenchmarkMethods(IPerformanceTimer timer)
    {
        _timer = timer;
    }
    public async Task<BenchmarkResult> RunBenchmarkAsync()
    {
        var results = new BenchmarkResult();
        
        // Sync processing
        var sw = Stopwatch.StartNew();
        await ProcessRecordsSync();
        sw.Stop();
        results.SyncTime = sw.ElapsedMilliseconds;
        
        // Async processing
        sw.Restart();
        await ProcessRecordsAsync();
        sw.Stop();
        results.AsyncTime = sw.ElapsedMilliseconds;
        
        // Parallel processing
        sw.Restart();
        await ProcessRecordsParallel();
        sw.Stop();
        results.ParallelTime = sw.ElapsedMilliseconds;
        
        // Parallel LINQ
        sw.Restart();
        await ProcessRecordsParallelLinq();
        sw.Stop();
        results.ParallelLinqTime = sw.ElapsedMilliseconds;
        
        return results;
    }
    
    private async Task ProcessRecordsSync()
    {
        var records = GenerateRecords(RecordCount);
        var results = new List<string>();
        
        foreach (var record in records)
        {
            var processed = await ProcessRecordAsync(record);
            results.Add(processed);
        }
    }
    
    private async Task ProcessRecordsAsync()
    {
        var records = GenerateRecords(RecordCount);
        var tasks = records.Select(ProcessRecordAsync);
        await Task.WhenAll(tasks);
    }
    
    private async Task ProcessRecordsParallel()
    {
        var records = GenerateRecords(RecordCount);
        var semaphore = new SemaphoreSlim(Environment.ProcessorCount * 2);
        var tasks = records.Select(async record =>
        {
            await semaphore.WaitAsync();
            try
            {
                await ProcessRecordAsync(record);
            }
            finally
            {
                semaphore.Release();
            }
        });
        
        await Task.WhenAll(tasks);
    }
    
    private async Task ProcessRecordsParallelLinq()
    {
        var records = GenerateRecords(RecordCount);
        await Task.Run(() =>
        {
            records.AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .ForAll(async record => await ProcessRecordAsync(record));
        });
    }
    
    private async Task<string> ProcessRecordAsync(string record)
    {
        // Simulate I/O operation
        await Task.Delay(1);
        return record.ToUpperInvariant();
    }
    
    private IEnumerable<string> GenerateRecords(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => $"Record-{i}-{Guid.NewGuid()}");
    }
    
    public async Task RunAsync()
    {
        var elapsed = await _timer.MeasureAsync(async () =>
        {
            await Task.Delay(100);
        });

        Console.WriteLine($"Elapsed: {elapsed.TotalMilliseconds} ms");
    }
}
public class BenchmarkResult
{
    public long SyncTime { get; set; }
    public long AsyncTime { get; set; }
    public long ParallelTime { get; set; }
    public long ParallelLinqTime { get; set; }
    
    public void PrintResults()
    {
        Console.WriteLine("=== Benchmark Results ===");
        Console.WriteLine($"Sync:           {SyncTime} ms");
        Console.WriteLine($"Async:          {AsyncTime} ms");
        Console.WriteLine($"Parallel:       {ParallelTime} ms");
        Console.WriteLine($"Parallel LINQ:  {ParallelLinqTime} ms");
        Console.WriteLine($"\nSpeedup (Async):        {SyncTime / (double)AsyncTime:F2}x");
        Console.WriteLine($"Speedup (Parallel):      {SyncTime / (double)ParallelTime:F2}x");
        Console.WriteLine($"Speedup (Parallel LINQ): {SyncTime / (double)ParallelLinqTime:F2}x");
    }
}
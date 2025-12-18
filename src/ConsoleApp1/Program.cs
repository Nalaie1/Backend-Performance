using ConsoleApp1.Day1_2;
using ConsoleApp1.Day3;
using ConsoleApp1.Day4.Services;
using ConsoleApp1.Day4.Interfaces;
using Microsoft.Extensions.DependencyInjection;

// /// Day 1-2: Word Counter
// Console.WriteLine("Day 1-2: Word Counter");
//
// var filePath =
//     @"C:/Users/Admin/RiderProjects/Backend-Performance/ConsoleApp1/bin/Debug/net9.0/big.log";
//
// var counter = new WordCounter();

// // Sequential
// var sequential = counter.CountWordsSequential(filePath);
// Console.WriteLine($"Xử lý tuần tự: {sequential.Count}");

// // Parallel
// var parallel = counter.CountWordsParallel(filePath);
// Console.WriteLine($"Xử lý song song: {parallel.Count}");


/// DI setup
var services = new ServiceCollection();

services.AddSingleton<IPerformanceTimer, PerformanceTimer>();
services.AddSingleton<BenchmarkMethods>();

var provider = services.BuildServiceProvider();

var benchmark = provider.GetRequiredService<BenchmarkMethods>();
var timer = provider.GetRequiredService<IPerformanceTimer>();


/// Day 3: Benchmark Methods
Console.WriteLine("\nDay 3: Benchmark Methods");

var benchmarkResult = await benchmark.RunBenchmarkAsync();
benchmarkResult.PrintResults();


/// Day 4: Performance Timer with Interface and Implementation
Console.WriteLine("\nDay 4: Performance Timer with Interface and Implementation");

Console.WriteLine("\n--- Sync void ---");
var elapsed1 = timer.Measure(() =>
{
    Thread.Sleep(500);
});
Console.WriteLine($"Elapsed: {elapsed1.TotalMilliseconds} ms");

Console.WriteLine("\n--- Sync return ---");
var sum = timer.Measure(() =>
{
    Thread.Sleep(300);
    return 1 + 2;
}, out var elapsed2);
Console.WriteLine($"Result: {sum}, Elapsed: {elapsed2.TotalMilliseconds} ms");

Console.WriteLine("\n--- Async void ---");
var elapsed3 = await timer.MeasureAsync(async () =>
{
    await Task.Delay(400);
});
Console.WriteLine($"Elapsed: {elapsed3.TotalMilliseconds} ms");

Console.WriteLine("\n--- Async return ---");
var (asyncResult, elapsed4) = await timer.MeasureAsync(async () =>
{
    await Task.Delay(200);
    return "Done";
});
Console.WriteLine($"Result: {asyncResult}, Elapsed: {elapsed4.TotalMilliseconds} ms");

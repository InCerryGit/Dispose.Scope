//Stress Test, Use httpclient 50 connection and 50 threads request http://localhost:8080/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

const int port = 5120;

// If not Windows, exit (PerformanceCounter is Windows-only).
if (!OperatingSystem.IsWindows())
{
    Console.WriteLine("This program is only for Windows.");
    return;
}

Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)0x3FC;

#if NET10_0
const string serverTfm = "net10.0";
#elif NET9_0
const string serverTfm = "net9.0";
#elif NET8_0
const string serverTfm = "net8.0";
#else
const string serverTfm = "net7.0";
#endif

var usePooled = "1";
var startInfo = new ProcessStartInfo
{
    FileName = $@"..\Benchmark.AspNetCore\bin\Release\{serverTfm}\Benchmark.AspNetCore.exe",
    RedirectStandardOutput = true,
    EnvironmentVariables =
    {
        {"ASPNETCORE_URLS", $"http://localhost:{port}"},
        {"System.GC.HeapAffinitizeMask", "C00"},
        {"DOTNET_gcServer","1"},
        {"DOTNET_gcConcurrent","1"},
        {"Use_Pooled",usePooled}
    }
};

using Process process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start Benchmark.AspNetCore.");
using var memoryUsage = new PerformanceCounter("Process", "Working Set - Private", process.ProcessName);
using var cpuUsage = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);

var cpuUsageList = new List<double>();
var memoryUsageList = new List<double>();
var cancelSource = new CancellationTokenSource();
var token = cancelSource.Token;

// wait for server start
Thread.Sleep(5000);

var url = usePooled == "1" ? $"http://localhost:{port}/foo/GetSomeClassesUsePooled" : $"http://localhost:{port}/foo/GetSomeClasses";

Console.WriteLine("Warm up start");
ThreadPool.SetMinThreads(100, 50);
var httpClients = Enumerable.Range(0, 50).Select(i =>
{
    var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
    httpClient.DefaultRequestHeaders.Add("keep-alive", "true");
    _ = httpClient.GetAsync(url).Result;
    return httpClient;
}).ToArray();

// warm up
await Parallel.ForEachAsync(Enumerable.Range(0, 10000), new ParallelOptions { MaxDegreeOfParallelism = 50 }, async (i, token) =>
{
    var httpClient = httpClients[i % 50];
    var response = await httpClient.GetAsync(url);
    _ = await response.Content.ReadAsStringAsync();
});
Console.WriteLine("Warm up done");

var monitorThread = new Thread(() =>
{
    // Keep the platform guard inside the thread for analyzers.
    if (!OperatingSystem.IsWindows())
    {
        return;
    }

    while (!token.IsCancellationRequested)
    {
        memoryUsageList.Add(memoryUsage.NextValue());
        cpuUsageList.Add(cpuUsage.NextValue());
        Thread.Sleep(50);
    }
})
{
    IsBackground = true
};
monitorThread.Start();

// wait for monitor start
Thread.Sleep(1000);

Console.WriteLine("Start stress test");
var count = 10000;

// test
var totalTime = Stopwatch.StartNew();
var singleRequestTimes = new ConcurrentBag<long>();
await Parallel.ForEachAsync(Enumerable.Range(0, count), new ParallelOptions { MaxDegreeOfParallelism = 50 }, async (i, token) =>
{
    var httpClient = httpClients[i % 50];
    var sw = Stopwatch.StartNew();
    var response = await httpClient.GetAsync(url);
    _ = await response.Content.ReadAsStringAsync();
    singleRequestTimes.Add(sw.ElapsedMilliseconds);
});

cancelSource.Cancel();
monitorThread.Join();

var totalTimeMs = totalTime.ElapsedMilliseconds;

Console.WriteLine(totalTimeMs);
Console.WriteLine(singleRequestTimes.Min());
Console.WriteLine(singleRequestTimes.Average());
Console.WriteLine(singleRequestTimes.Max());
Console.WriteLine(count / (totalTimeMs / 1000.0));

// calculate p95 p99
var singleRequestTimesSortMin = singleRequestTimes.OrderBy(x => x).ToList();
var p95 = singleRequestTimesSortMin[(int)Math.Ceiling(singleRequestTimesSortMin.Count * 0.95) - 1];
var p99 = singleRequestTimesSortMin[(int)Math.Ceiling(singleRequestTimesSortMin.Count * 0.99) - 1];
Console.WriteLine(p95);
Console.WriteLine(p99);

var cpu = cpuUsageList.Where(c => c > 0).ToArray();
Console.WriteLine(cpu.Length == 0 ? 0 : cpu.Average());
Console.WriteLine(cpu.Length == 0 ? 0 : cpu.Max());

var memory = memoryUsageList.Where(c => c > 0).ToArray();
Console.WriteLine(memory.Length == 0 ? 0 : memory.Average());
Console.WriteLine(memory.Length == 0 ? 0 : memory.Max());

Console.ReadLine();

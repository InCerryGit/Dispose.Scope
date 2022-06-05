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

// if not windows os, return
if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false)
{
    Console.WriteLine("This program is only for windows os");
    return;
}

Process.GetCurrentProcess().ProcessorAffinity = (IntPtr) 0x3FC;
var usePooled = "1";
var startInfo = new ProcessStartInfo
{
    FileName = @"..\Benchmark.Aspnetcore\bin\Release\net6.0\Benchmark.Aspnetcore.exe",
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
var process = Process.Start(startInfo);
var memoryUsage = new PerformanceCounter("Process", "Working Set - Private", process.ProcessName);
var cpuUsage = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);
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

new Thread(() =>
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        while (token.IsCancellationRequested == false)
        {
            memoryUsageList.Add(memoryUsage.NextValue());
            cpuUsageList.Add(cpuUsage.NextValue());
            Thread.Sleep(50);
        }
    }
}).Start();
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
Console.WriteLine(cpuUsageList.Where(c => c > 0).Average());
Console.WriteLine(cpuUsageList.Where(c => c > 0).Max());
Console.WriteLine(memoryUsageList.Where(c => c > 0).Average());
Console.WriteLine(memoryUsageList.Where(c => c > 0).Max());
Console.ReadLine();
cancelSource.Cancel();
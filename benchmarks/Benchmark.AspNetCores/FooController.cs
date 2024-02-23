using Collections.Pooled;
using Dispose.Scope;
using Microsoft.AspNetCore.Mvc;

namespace Benchmark.AspNetCore;

[ApiController]
[Route("[controller]/[action]")]
public class FooController : Controller
{
    private static readonly SomeClassViewModel[] SomeClasses = Enumerable.Range(0, 10000).Select(i => new SomeClassViewModel
    {
        F1 = i, F2 = i, F3 = i.ToString(), F4 = i.ToString(), F5 = i, F6 = i
    }).ToArray();
    

    [HttpGet]
    public IList<SomeClassViewModel> GetSomeClasses()
    {
        var result = new List<SomeClassViewModel>();
        for (int i = 0; i < SomeClasses.Length; i++)
        {
            result.Add(SomeClasses[i]);
        }

        return result;
    }

    [HttpGet]
    public IList<SomeClassViewModel> GetSomeClassesUsePooled()
    {
        var result = new PooledList<SomeClassViewModel>().RegisterDisposeScope();
        for (int i = 0; i < SomeClasses.Length; i++)
        {
            result.Add(SomeClasses[i]);
        }

        return result;
    }
}

public class SomeClassViewModel
{
    public int F1 { get; set; }
    public int F2 { get; set; }
    public string? F3 { get; set; }
    public string? F4 { get; set; }
    public double F5 { get; set; }
    public float F6 { get; set; }
}
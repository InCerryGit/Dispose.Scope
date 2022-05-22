using Dispose.Scope;
using Microsoft.AspNetCore.Mvc;

namespace Benchmark.Aspnetcore;

[ApiController]
[Route("[controller]/[action]")]
public class FooController : Controller
{
    private static readonly SomeClass[] SomeClasses = Enumerable.Range(0, 1000).Select(i => new SomeClass
    {
        F1 = i, F2 = i, F3 = i.ToString(), F4 = i.ToString(), F5 = i, F6 = i
    }).ToArray();
    
    [HttpGet]
    public IList<SomeClass> GetSomeClasses()
    {
        return SomeClasses.ToList();
    }

    [HttpGet]
    public IList<SomeClass> GetSomeClassesUsePooled()
    {
        return SomeClasses.ToPooledListScope();
    }
}

public class SomeClass
{
    public int F1 {get; set;}
    public int F2 {get; set;}
    public string? F3 {get; set;}
    public string? F4 {get; set;}
    public double F5 {get; set;}
    public float F6 {get; set;}
}
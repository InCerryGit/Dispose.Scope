<div align="center">
    <strong><a href="README.md">简体中文</a> | <a href="README.en-US.md">English</a></strong>
</div>


# Dispose.Scope
`Dispose.Scope`is a class library that allows you to easily manage instances of objects that implement the `IDisposeable` interface using scopes. register the `IDisposable` instances that need to be released into the scope, and then automatically release all the registered obejcts at the end of the scope.

## Usage
`Dispose.Scope` very easy to use. First install the NuGet package：

[NuGet](https://www.nuget.org/packages/Dispose.Scope/)

```ini
Install-Package Dispose.Scope
dotnet add package Dispose.Scope
paket add Dispose.Scope
```
you can use the `Dispose.Scope` API directly, all the samples in this article you can find in the `samples` folder, for example we have a class called `NeedDispose` with the following code.

```csharp
public class NeedDispose : IDisposable
{
    public NeedDispose(string name)
    {
        Name = name;
    }
    public string Name { get; set; }
    
    public void Dispose()
    {
        Console.WriteLine("Dispose");
    }
}
```
We can then use `DisposeScope` like the following:
```csharp
using Dispose.Scope;

using (var scope = DisposeScope.BeginScope())
{
    var needDispose = new NeedDisposeClass("A1");
    // register to current scope
    needDispose.RegisterDisposeScope();
}
// output: A1 Is Dispose
```
Similarly, `DisposeScope` can be used in an asynchronous context:
```csharp
using (var scope = DisposeScope.BeginScope())
{
    await Task.Run(() =>
    {
        var needDispose = new NeedDispose("A2");
        // register to current scope
        needDispose.RegisterDisposeScope();
    });
}
// output: A2 Is Dispose
```
We can nest multiple `DisposeScope` in the scope of a `DisposeScope`, if there is a `DisposeScope` in the context then it will use the `DisposeScope` in the context, if not then they will create a new.
```csharp
using (_ = DisposeScope.BeginScope())
{
    var d0 = new NeedDispose("D0").RegisterDisposeScope();
    
    using (_ = DisposeScope.BeginScope())
    {
        var d1 = new NeedDispose("D1").RegisterDisposeScope();
    }
    using (_ = DisposeScope.BeginScope())
    {
        var d2 = new NeedDispose("D2").RegisterDisposeScope();
    }
}
// output:
// D0 is Dispose
// D1 is Dispose
// D2 is Dispose

```
If you want nested scopes to be released first, then the scope needs to specify `DisposeScopeOption.RequiresNew` when calling the `BeginScope` method (see below for the `DisposeScopeOption` option), which will create a new scope regardless of whether there is a scope in the context or not a new scope:
```csharp
using (_ = DisposeScope.BeginScope())
{
    var d0 = new NeedDispose("D0").RegisterDisposeScope();
   
    using (_ = DisposeScope.BeginScope(DisposeScopeOption.RequiresNew))
    {
        var d1 = new NeedDispose("D1").RegisterDisposeScope();
    }
    using (_ = DisposeScope.BeginScope(DisposeScopeOption.RequiresNew))
    {
        var d2 = new NeedDispose("D2").RegisterDisposeScope();
    }
}
// output:
// D1 Is Dispose
// D2 Is Dispose
// D0 Is Dispose
```
If you don't want to use `DisposeScope` in a nested scope, then you can specify `DisposeScopeOption.Suppress` and it will ignore the context's `DisposeScope`, but if you use ` RegisterDisposeScope`, an exception will be thrown by default: 

```csharp
using (_ = DisposeScope.BeginScope())
{
    var d0 = new NeedDispose("D0").RegisterDisposeScope();
    
    using (_ = DisposeScope.BeginScope(DisposeScopeOption.RequiresNew))
    {
        var d1 = new NeedDispose("D1").RegisterDisposeScope();
    }
    using (_ = DisposeScope.BeginScope(DisposeScopeOption.Suppress))
    {
        // was throw exception, because this context is not DisposeScope
        var d2 = new NeedDispose("D2").RegisterDisposeScope();
    }
}
// output:
// System.InvalidOperationException: Can not use Register on not DisposeScope context
//    at Dispose.Scope.DisposeScope.Register(IDisposable disposable) in E:\MyCode\PooledScope\src\Dispose.Scope\DisposeScope.cs:line 100
//    at Program.<<Main>$>g__Method3|0_4() in E:\MyCode\PooledScope\Samples\Sample\Program.cs:line 87
//    at Program.<Main>$(String[] args) in E:\MyCode\PooledScope\Samples\Sample\Program.cs:line 9

```
If you don't want it to throw an exception, then just set `DisposeScope.ThrowExceptionWhenNotHaveDisposeScope = false` globally at the beginning, and no exception will be thrown in contexts where there is no `DisposeScope`.
```csharp
// set false, no exceptions will be thrown
DisposeScope.ThrowExceptionWhenNotHaveDisposeScope = false;
using (_ = DisposeScope.BeginScope())
{
    var d0 = new NeedDispose("D0").RegisterDisposeScope();
    
    using (_ = DisposeScope.BeginScope(DisposeScopeOption.RequiresNew))
    {
        var d1 = new NeedDispose("D1").RegisterDisposeScope();
    }
    using (_ = DisposeScope.BeginScope(DisposeScopeOption.Suppress))
    {
        // no exceptions will be thrown
        var d2 = new NeedDispose("D2").RegisterDisposeScope();
    }
}
// output:
// D1 Is Dispose
// D0 Is Dispose
```

**DisposeScopeOption**

| 枚举                             | 描述                                                         |
| -------------------------------- | ------------------------------------------------------------ |
| `DisposeScopeOption.Required`    | ` DisposeScope` is required in the scope. if it already exists, it uses the environment`DisposeScope`. otherwise, it creates a new `DisposeScope` before entering the scope. this is the default value. |
| `DisposeScopeOption.RequiresNew` | Always create a new `DisposeScope`                           |
| `DisposeScopeOption.Suppress`    | The environment DisposeScope context is suppressed when creating a scope. All operations within the scope are done without the environment DisposeScope context. |

### Collections.Pooled Extension
The original intent of this project was to make it easier to use [Collections.Pooled](https://github.com/jtmueller/Collections.Pooled)，It is based on the official `System.Collections.Generic` and implements a collection object allocation based on `System.Buffers.ArrayPool`.
Pool-based collection object creation has very good performance and a very low memory footprint. But you need to do `Dispose` for it manually in use, which is fine in a single method, sometimes you will span multiple methods, which can be troublesome to write, and sometimes you forget to release it and lose the point of using Pool, as follows: 

```csharp
using Collections.Pooled;

Console.WriteLine(GetTotalAmount());

decimal GetTotalAmount()
{
    // forget to dispose `MethodB` result
    var result = GetRecordList().Sum(x => x.Amount);
    return result;
}

PooledList<Record> GetRecordList()
{
    // register to dispose scope
    var list = DbContext.Get().ToPooledList();
    return list;
}

```
Now you can add the `Dispose.Scope` class library so that you can set a `Scope` in the periphery and when the method ends, the objects registered in the scope will `Dispose`.
```csharp
using Dispose.Scope;
using Collections.Pooled;

// dispose the scope all registered objects
using(_ = DisposeScope.BeginScope)
{
    Console.WriteLine(GetTotalAmount());
}

decimal GetTotalAmount()
{
    // forget to dispose `MethodB` result, but don't worries, it will be disposed automatically
    var result = GetRecordList().Sum(x => x.Amount);
    return result;
}

PooledList<Record> GetRecordList()
{
    // register to dispose scope, it will be disposed automatically
    var list = DbContext.Get().ToPooledList().RegisterDisposeScope();
    // or
    var list = DbContext.Get().ToPooledListScope();
    return list;
}
```
#### Performance
``` ini
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK=6.0.203
  [Host]     : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT
  DefaultJob : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT
```
|                     Method |     Mean |   Error |  StdDev | Ratio | RatioSD |       Gen 0 |      Gen 1 |      Gen 2 | Allocated |
|--------------------------- |---------:|--------:|--------:|------:|--------:|------------:|-----------:|-----------:|----------:|
| GetSomeClassUsePooledUsing | 169.4 ms | 1.60 ms | 1.50 ms |  0.70 |    0.01 |  53333.3333 | 24333.3333 |          - |    305 MB |
| GetSomeClassUsePooledScope | 169.6 ms | 1.47 ms | 1.30 ms |  0.70 |    0.01 |  53000.0000 | 24333.3333 |          - |    306 MB |
|               GetSomeClass | 240.9 ms | 1.92 ms | 1.60 ms |  1.00 |    0.00 | 112333.3333 | 58000.0000 | 41333.3333 |    632 MB |
|      GetSomeClassUsePooled | 402.2 ms | 7.78 ms | 8.96 ms |  1.68 |    0.03 |  83000.0000 | 83000.0000 | 83000.0000 |    556 MB |

`GetSomeClassUsePooledScope` in the table is the performance of using `Dispose.Scope`, you can see that it is basically the same as manually `using`, with a little extra overhead of needing to create `DisposeScope` objects.

### Asp.Net Core Extension
Install NuGet Package `Dispose.Scope.AspNetCore`.
[NuGet](https://www.nuget.org/packages/Dispose.Scope.AspNetCore/)

```ini
Install-Package Dispose.Scope.AspNetCore
dotnet add package Dispose.Scope.AspNetCore
paket add Dispose.Scope.AspNetCore
```
In Asp.Net Core, the return to the Client side is a collection type that requires Json serialization, this scenario is not very good to use `Collections.Pooled`, because you need to release it at the end of the request processing, but you can not conveniently modify the code in the framework, as follows.
```csharp
using Collections.Pooled;

[ApiController]
[Route("api/[controller]")]
public class RecordController : Controller
{
    // you can't dispose PooledList<Record>
    PooledList<Record> GetRecordList(string id)
    {
        return RecordDal.Get(id);
    }
}
......
public class RecordDal
{
    public PooledList<Record> Get(string id)
    {
        var result = DbContext().Get(r => r.id == id).ToPooledList();
        return result;
    }
}
```
Now, you can use `Dispose.Scope.AspNetCore` package and register it as the first middleware (actually, just before you use the Pooled type), then use the `ToPooledListScope` or `RegisterDisposeScope` methods; this way, when the framework's request processing ends, it will automatically release all the registered objects.

```csharp
using Dispose.Scope.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var app = builder.Build();
// register UseDisposeScopeMiddleware
// it will be create a scope when http request begin, and dispose it when http request end
app.UseDisposeScope();
app.MapGet("/", () => "Hello World!");
app.MapControllers();
app.Run();

......

[ApiController]
[Route("api/[controller]")]
public class RecordController : Controller
{
    PooledList<Record> GetRecordList(string id)
    {
        return RecordDal.Get(id);
    }
}

......
public class RecordDal
{
    public PooledList<Record> Get(string id)
    {
        // use `ToPooledListScope` to register to dispose scope
        // will be dispose automatically when the scope is disposed
        var result = DbContext().Get(r => r.id == id).ToPooledListScope();
        return result;
    }
}
```
#### Performance
In ASP.NET Core `DisposeScope` and `PooledList` were used, also the normal `List` was used as a control group. The results of the crush test using the `https://github.com/InCerryGit/Dispose.Scope/tree/master/benchmarks` code are as follows.
>Server：1 Core  
>Client：5 Core
>Since the CPU affinity is used to tie the nucleus, there is the case of Client grabbing the CPU resources of Server, the conclusion is for reference only.

|Project|Total time (ms)|Min time (ms)|Avg time (ms)|Max time (ms)|QPS|P95 latency|P99 latency|Memory Used|
|----|----|----|----|----|----|----|----|----|
|DisposeScope+PooledList|1997|1|9.4|80|5007|19|31|59MB|
|List|2019|1|9.5|77|4900|19|31|110MB|

By averaging the values a few times, the scenario using `Dispose.Scope` combined with `PooledList` has a 53% lower memory footprint, about 2% higher QPS, and basically no sexual regression in other metrics.

## Precautions
One scenario to be aware of when using `Dispose.Scope` is when there are cross-threaded operations within the scope, such as the following example.
```csharp
using Dispose.Scope;

using(var scope = DisposeScope.BeginScope())
{
    // do something
    _ = Task.Run(() =>
    {
        // do something
        var list = new PooledList<Record>().RegisterDisposeScope();
    });
}
```
There is a serious problem with the above code, when the outer scope ends, it may cause the object to be released incorrectly before the tasks of other threads inside are finished. If you encounter such a scenario, you should suppress the `DisposeScope` in the context and then recreate the scope in the other thread.
```csharp
using Dispose.Scope;

using(var scope = DisposeScope.BeginScope())
{
    // suppress context scope
    using(var scope2 = DisposeScope.BeginScope(DisposeScopeOption.Suppress))
    {
        _ = Task.Run(() =>
        {
            // on other thread create new scope
            using(var scope = DisposeScope.BeginScope())
            {
                // do something
                var list = new PooledList<Record>().RegisterDisposeScope();
            }
        });
    }

}
```


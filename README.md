# Dispose.Scope
`Dispose.Scope`是一个可以让你方便的使用作用域管理实现了`IDisposable`接口的对象实例的类库。将需要释放的`IDisposable`注册到作用域中，然后在作用域结束时自动释放所有注册的对象。

## 使用方式
`Dispose.Scope`使用非常简单，只需要几步就能完成上文中提到的功能。首先安装Nuget包：

[NuGet](https://www.nuget.org/packages/Dispose.Scope/)

```ini
Install-Package Dispose.Scope
dotnet add package Dispose.Scope
paket add Dispose.Scope
```
你可以直接使用`Dispose.Scope`的API，本文的所有样例你都可以在`samples`文件夹中找到，比如我们有一个类叫`NeedDispose`代码如下：
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
然后我们就可以像下面这样使用`DisposeScope`：
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
同样，在异步上下文中也可以使用`DisposeScope`：
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
当然我们可以在一个`DisposeScope`的作用域当中，嵌套多个`DisposeScope`，如果上下文中存在`DisposeScope`那么他们会直接使用上下文中的，如果没有那么他们会创建一个新的。
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
如果你想让嵌套的作用域优先释放，那么作用域调用`BeginScope`方法时需要指定`DisposeScopeOption.RequiresNew`（关于`DisposeScopeOption`选项可以查看下面的的内容），它不管上下文中有没有作用域，都会创建一个新的作用域：
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
如果你不想在嵌套作用域中使用`DisposeScope`，那么可以指定`DisposeScopeOption.Suppress`，它会忽略上下文的`DisposeScope`，但是如果你在没有`DisposeScope`上下文中使用`RegisterDisposeScope`，默认会抛出异常。

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
如果不想让它抛出异常，那么只需要在开始全局设置`DisposeScope.ThrowExceptionWhenNotHaveDisposeScope = false`,在没有`DisposeScope`的上下文中，也不会抛出异常.
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
| `DisposeScopeOption.Required`    | 作用域内需要 DisposeScope。如果已经存在，它使用环境 DisposeScope。否则，它会在进入作用域之前创建一个新的 DisposeScope。这是默认值。 |
| `DisposeScopeOption.RequiresNew` | 无论环境中是否有 DisposeScope，始终创建一个新的 DisposeScope |
| `DisposeScopeOption.Suppress`    | 创建作用域时会抑制环境 DisposeScope 上下文。作用域内的所有操作都是在没有环境 DisposeScope 上下文的情况下完成的。 |

### Collections.Pooled扩展
本项目一开始的初衷就是为了更方面的使用[Collections.Pooled](https://github.com/jtmueller/Collections.Pooled)，它基于官方的`System.Collections.Generic`，实现了基于`System.Buffers.ArrayPool`的集合对象分配。
基于池的集合对象生成有着非常好的性能和非常低的内存占用。但是您在使用中需要手动为它进行`Dispose`，这在单一的方法中还好，有时您会跨多个方法，写起来会比较麻烦，而且有时会忘记去释放它，失去了使用Pool的意义，如下所示：

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
现在您可以添加`Dispose.Scope`的类库，这样可以在外围设置一个`Scope`，当方法结束时，作用域内注册的对象都会`Dispose`。
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
#### 性能
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

表格中`GetSomeClassUsePooledScope`就是使用`Dispose.Scope`的性能，可以看到它基本和手动`using`一样，稍微有一点额外的开销就是需要创建`DisposeScope`对象。
### Asp.Net Core扩展
安装Nuget包`Dispose.Scope.AspNetCore`.
[NuGet](https://www.nuget.org/packages/Dispose.Scope.AspNetCore/)

```ini
Install-Package Dispose.Scope.AspNetCore
dotnet add package Dispose.Scope.AspNetCore
paket add Dispose.Scope.AspNetCore
```
在Asp.Net Core中，返回给Client端是需要Json序列化的集合类型，这种场景下不太好使用`Collections.Pooled`，因为你需要在请求处理结束时释放它，但是你不能方便的修改框架中的代码，如下所示：
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
现在你可以引用`Dispose.Scope.AspNetCore`包，然后将它注册为第一个中间件（其实只要在你使用Pooled类型之前即可），然后使用`ToPooledListScope`或者`RegisterDisposeScope`方法；这样在框架的求处理结束时，它会自动释放所有注册的对象。

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
#### 性能
在ASP.NET Core使用了`DisposeScope`和`PooledList`，也使用普通的`List`作为对照组。使用`https://github.com/InCerryGit/Dispose.Scope/tree/master/benchmarks`代码进行压测，结果如下：
>**机器配置**
>Server：1 Core  
>Client：5 Core
>由于是使用CPU亲和性进行绑核，存在Client抢占Server的Cpu资源的情况，结论仅供参考。

|项目|总耗时|最小耗时|平均耗时|最大耗时|QPS|P95延时|P99延时|内存占用率|
|----|----|----|----|----|----|----|----|----|
|DisposeScope+PooledList|1997|1|9.4|80|5007|19|31|59MB|
|List|2019|1|9.5|77|4900|19|31|110MB|

通过几次平均取值，使用`Dispose.Scope`结合`PooledList`的场景，内存占用率要低53%，QPS高了2%左右，其它指标基本没有任何性的退步。

## 注意
在使用`Dispose.Scope`需要注意一个场景，那就是在作用域内有跨线程操作时，比如下面的例子：
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
上面的代码存在严重的问题，当外层的作用域结束时，可能内部其它线程的任务还未结束，就会导致对象错误的被释放。如果您遇到这样的场景，您应该抑制上下文中的`DisposeScope`，然后在其它线程中重新创建作用域。
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


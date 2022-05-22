using Dispose.Scope;

Method();
await MethodAsync();
Method1();
Method2();
Method4();

try
{
    Method3();
}
catch (Exception e)
{
    Console.WriteLine(e);
}

void Method()
{
    using (var scope = DisposeScope.BeginScope())
    {
        var a1 = new NeedDispose("A1")
            .RegisterDisposeScope(); // register to current scope
    }
}

async Task MethodAsync()
{
    using (var scope = DisposeScope.BeginScope())
    {
        await Task.Run(() =>
        {
            var a2 = new NeedDispose("A2")
                .RegisterDisposeScope(); // register to current scope
        });
    }
}

void Method1()
{
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
}

void Method2()
{
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
}

void Method3()
{
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
}

void Method4()
{
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
}

public class NeedDispose : IDisposable
{
    public NeedDispose(string name)
    {
        Name = name;
    }
    public string Name { get; set; }
    
    public void Dispose()
    {
        Console.WriteLine($"{Name} is Dispose");
    }
}
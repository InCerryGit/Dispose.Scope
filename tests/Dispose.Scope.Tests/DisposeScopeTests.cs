using System;
using System.Threading.Tasks;
using Dispose.Scope;
using Xunit;

namespace PooledScope.Tests;

public class DisposeScopeTests
{
    [Fact]
    public void Throw_Exception_When_Context_Not_DisposeScope_And_ThrowExceptionWhenNotHaveDisposeScope_Is_True()
    {
        DisposeScope.ThrowExceptionWhenNotHaveDisposeScope = true;
        Assert.Null(DisposeScope.Current.Value);
        Assert.Throws<InvalidOperationException>(() =>
        {
            var _ = new Class().RegisterDisposeScope();
        });
        Assert.Throws<InvalidOperationException>(() =>
        {
            var _ = new Class().UnRegisterFormDisposeScope();
        });
    }

    [Fact]
    public void No_Exception_Was_Throw_When_Context_Not_DisposeScope_And_ThrowExceptionWhenNotHaveDisposeScope_Is_True()
    {
        DisposeScope.ThrowExceptionWhenNotHaveDisposeScope = false;
        Assert.Null(DisposeScope.Current.Value);
        Assert.NotNull(new Class().RegisterDisposeScope());
        Assert.NotNull(new Class().UnRegisterFormDisposeScope());
    }
    
    [Fact]
    public void Call_RegisterDisposeScope_Do_Not_Do_Anything_When_Object_Is_Null()
    {
        Class class1 = null;
        using (var scope = DisposeScope.BeginScope())
        {
            class1!.RegisterDisposeScope();
            Assert.Empty(scope._currentScopeDisposables!);
        }
    }
    
    [Fact]
    public void Call_UnRegisterDisposeScope_Do_Not_Do_Anything_When_Object_Is_Null()
    {
        Class class1 = null;
        using (var scope = DisposeScope.BeginScope())
        {
            class1!.RegisterDisposeScope();
            Assert.Empty(scope._currentScopeDisposables!);
            class1!.UnRegisterFormDisposeScope();
            Assert.Empty(scope._currentScopeDisposables!);
        }
    }

    [Fact]
    public void Should_Remove_On_Context_DisposeScope_When_Call_UnRegisterFormDisposeScope()
    {
        var class1 = new Class();
        using (var scope = DisposeScope.BeginScope())
        {
            class1.RegisterDisposeScope();
            Assert.Single(scope._currentScopeDisposables!);
            Assert.Equal(scope._currentScopeDisposables![0], class1);
            class1.UnRegisterFormDisposeScope();
            Assert.Empty(scope._currentScopeDisposables!);
        }
    }

    [Theory]
    [InlineData(DisposeScopeOption.Required, false)]
    [InlineData(DisposeScopeOption.RequiresNew, false)]
    [InlineData(DisposeScopeOption.Suppress, true)]
    public void From_Option_Create_New_Scope_When_Context_Not_DisposeScope(DisposeScopeOption option, bool shouldNull)
    {
        DisposeScope.Current.Value = null;
        using (var scope1 = DisposeScope.BeginScope(option))
        {
            if (shouldNull)
            {
                Assert.Null(DisposeScope.Current.Value);
            }
            else
            {
                Assert.NotNull(DisposeScope.Current.Value);
            }
        }
    }

    [Theory]
    [InlineData(DisposeScopeOption.Required, false)]
    [InlineData(DisposeScopeOption.RequiresNew, false)]
    [InlineData(DisposeScopeOption.Suppress, true)]
    public async Task From_Option_Create_New_Scope_When_On_Async_Context_Not_DisposeScope(DisposeScopeOption option,
        bool shouldNull)
    {
        DisposeScope.Current.Value = null;
        using (var scope = DisposeScope.BeginScope(option))
        {
            await Task.Run(() =>
            {
                if (shouldNull)
                {
                    Assert.Null(DisposeScope.Current.Value);
                }
                else
                {
                    Assert.NotNull(DisposeScope.Current.Value);
                }
            });
        }
    }

    [Fact]
    public async Task Should_Clear_Context_DisposeScope_When_DisposeScopeOption_Is_Suppress()
    {
        using (var scope = DisposeScope.BeginScope())
        {
            using (var _ = DisposeScope.BeginScope(DisposeScopeOption.Suppress))
            {
                await Task.Run(() => { Assert.Null(DisposeScope.Current.Value); });
                Assert.Null(DisposeScope.Current.Value);
            }
            Assert.Equal(scope, DisposeScope.Current.Value);
        }
    }

    [Fact]
    public async Task Should_Renew_Context_DisposeScope_When_DisposeScopeOption_Is_Suppress()
    {
        using (var scope = DisposeScope.BeginScope())
        {
            using (var _ = DisposeScope.BeginScope(DisposeScopeOption.RequiresNew))
            {
                await Task.Run(() =>
                {
                    Assert.NotNull(DisposeScope.Current.Value);
                    Assert.NotEqual(scope, DisposeScope.Current.Value);
                });
                
                Assert.NotNull(DisposeScope.Current.Value);
                Assert.NotEqual(scope, DisposeScope.Current.Value);
            }
            Assert.Equal(scope, DisposeScope.Current.Value);
        }
    }

    [Fact]
    public async Task Should_Requires_Context_DisposeScope_When_DisposeScopeOption_Is_Suppress()
    {
        using (var scope = DisposeScope.BeginScope())
        {
            using (var _ = DisposeScope.BeginScope(DisposeScopeOption.Required))
            {
                await Task.Run(() =>
                {
                    Assert.NotNull(DisposeScope.Current.Value);
                    Assert.Equal(scope, DisposeScope.Current.Value);
                });
                
                Assert.NotNull(DisposeScope.Current.Value);
                Assert.Equal(scope, DisposeScope.Current.Value);
            }
            Assert.Equal(scope, DisposeScope.Current.Value);
        }
    }
    
    [Fact]
    public async Task Should_Dispose_Register_To_Dispose_Object()
    {
        var obj = new Class();
        using (var scope = DisposeScope.BeginScope())
        {
            await Task.Run(() =>
            {
                obj.RegisterDisposeScope();
            });
            Assert.Single(scope._currentScopeDisposables!);
            Assert.Equal(scope._currentScopeDisposables![0], obj);
        }
        Assert.True(obj.IsDisposed);
    }
    
    [Fact]
    public async Task Should_Dispose_Register_To_Dispose_Object_On_Nested()
    {
        var obj = new Class();
        var obj1 = new Class();
        using (var scope = DisposeScope.BeginScope())
        {
            obj.RegisterDisposeScope();
            using (var scope1 = DisposeScope.BeginScope())
            {
                await Task.Run(() => { obj1.RegisterDisposeScope(); });
                Assert.Null(scope1._currentScopeDisposables);
                Assert.Equal(2, DisposeScope.Current.Value!._currentScopeDisposables!.Count);
                Assert.Equal(DisposeScope.Current.Value!._currentScopeDisposables![0], obj);
                Assert.Equal(DisposeScope.Current.Value!._currentScopeDisposables![1], obj1);
            }
            Assert.False(obj.IsDisposed);
            Assert.False(obj1.IsDisposed);
            Assert.Equal(2, DisposeScope.Current.Value!._currentScopeDisposables!.Count);
            Assert.Equal(DisposeScope.Current.Value!._currentScopeDisposables![0], obj);
            Assert.Equal(DisposeScope.Current.Value!._currentScopeDisposables![1], obj1);
        }
        Assert.True(obj.IsDisposed);
        Assert.True(obj1.IsDisposed);
    }
    
    [Fact]
    public async Task Should_Dispose_Register_To_Dispose_Object_On_Nested_RequiresNew()
    {
        var obj = new Class();
        var obj1 = new Class();
        using (var scope = DisposeScope.BeginScope())
        {
            obj.RegisterDisposeScope();
            using (var scope1 = DisposeScope.BeginScope(DisposeScopeOption.RequiresNew))
            {
                await Task.Run(() => { obj1.RegisterDisposeScope(); });
                Assert.NotNull(scope1._currentScopeDisposables);
                Assert.Single(DisposeScope.Current.Value!._currentScopeDisposables!);
                Assert.Equal(DisposeScope.Current.Value!._currentScopeDisposables![0], obj1);
            }
            Assert.False(obj.IsDisposed);
            Assert.True(obj1.IsDisposed);
            Assert.Single(DisposeScope.Current.Value!._currentScopeDisposables!);
            Assert.Equal(DisposeScope.Current.Value!._currentScopeDisposables![0], obj);
        }
        Assert.True(obj.IsDisposed);
        Assert.True(obj1.IsDisposed);
    }
}

public class Class : IDisposable
{
    public bool IsDisposed { get; set; }
    
    public Action? DisposeAction { get; set; } 
    
    public void Dispose()
    {
        DisposeAction?.Invoke();
        IsDisposed = true;
    }
}
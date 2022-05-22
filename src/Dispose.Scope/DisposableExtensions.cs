using System;
using System.Runtime.CompilerServices;

namespace Dispose.Scope
{
    public static class DisposableExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RegisterDisposeScope<T>(this T disposable) where T : IDisposable
        {
            DisposeScope.Register(disposable);
            return disposable;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T UnRegisterFormDisposeScope<T>(this T disposable) where T : IDisposable
        {
            DisposeScope.UnRegister(disposable);
            return disposable;
        }
    }
}
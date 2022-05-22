using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Collections.Pooled;

namespace Dispose.Scope
{
    public sealed class DisposeScope : IDisposable
    {
        /// <summary>
        /// Throw exception when call Register context not have DisposeScope, default is true.
        /// </summary>
        public static bool ThrowExceptionWhenNotHaveDisposeScope { get; set; } = true;

#if DEBUG
        internal
#else
    private
#endif
            static readonly AsyncLocal<DisposeScope> Current = new AsyncLocal<DisposeScope>();

#if DEBUG
        internal
#else
    private
#endif
            DisposeScopeOption Option { get; }

#if DEBUG
        internal
#else
    private
#endif
            readonly DisposeScope _before;

#if DEBUG
        internal
#else
    private
#endif
            readonly PooledList<IDisposable> _currentScopeDisposables;

        /// <summary>
        /// Create new DisposeScope.
        /// </summary>
        public DisposeScope() : this(DisposeScopeOption.Required, 8)
        {
        }

        /// <summary>
        /// Create DisposeScope with option.
        /// </summary>
        /// <param name="option">see <see cref="DisposeScopeOption"/></param>
        /// <param name="size">the size of _currentScopeDisposables</param>
        public DisposeScope(DisposeScopeOption option, int size)
        {
            Option = option;
            _before = Current.Value;
            switch (Option)
            {
                case DisposeScopeOption.Suppress:
                    Current.Value = null;
                    break;
                case DisposeScopeOption.RequiresNew:
                    _currentScopeDisposables = new PooledList<IDisposable>(size);
                    Current.Value = this;
                    break;
                case DisposeScopeOption.Required:
                default:
                    if (Current.Value is null)
                    {
                        _currentScopeDisposables = new PooledList<IDisposable>(size);
                        Current.Value = this;
                    }

                    break;
            }
        }

        private void AddToScope(IDisposable disposable)
        {
            _currentScopeDisposables?.Add(disposable);
        }

        private void RemoveFromScope(IDisposable disposable)
        {
            _currentScopeDisposables?.Remove(disposable);
        }

        /// <summary>
        /// Register disposable to current DisposeScope.
        /// </summary>
        /// <param name="disposable">implement <see cref="IDisposable"/> object</param>
        /// <exception cref="InvalidOperationException">if this context not have DisposeScope and <see cref="ThrowExceptionWhenNotHaveDisposeScope"/> is true.</exception>
        public static void Register(IDisposable disposable)
        {
            if (disposable is null) return;
            if (Current.Value is null)
            {
                if (ThrowExceptionWhenNotHaveDisposeScope)
                {
                    throw new InvalidOperationException("Can not use Register on not DisposeScope context");
                }

                return;
            }

            Current.Value.AddToScope(disposable);
        }

        /// <summary>
        /// Unregister disposable from current DisposeScope.
        /// </summary>
        /// <param name="disposable">implement <see cref="IDisposable"/> object</param>
        /// <exception cref="InvalidOperationException">if this context not have DisposeScope and <see cref="ThrowExceptionWhenNotHaveDisposeScope"/> is true.</exception>
        public static void UnRegister(IDisposable disposable)
        {
            if (disposable is null) return;
            if (Current.Value is null)
            {
                if (ThrowExceptionWhenNotHaveDisposeScope)
                {
                    throw new InvalidOperationException("Can not use UnRegister on not DisposeScope context");
                }

                return;
            }

            Current.Value.RemoveFromScope(disposable);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DisposeScope BeginScope(DisposeScopeOption option = DisposeScopeOption.Required,
            int disposeObjListDefaultSize = 8)
        {
            return new DisposeScope(option, disposeObjListDefaultSize);
        }

        public void Dispose()
        {
            if (_currentScopeDisposables != null)
            {
                for (var index = 0; index < _currentScopeDisposables.Count; index++)
                {
                    _currentScopeDisposables[index].Dispose();
                }

                _currentScopeDisposables.Clear();
                _currentScopeDisposables.Dispose();
            }

            Current.Value = _before;
        }
    }
}
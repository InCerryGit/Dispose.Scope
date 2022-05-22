using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Collections.Pooled;

namespace Dispose.Scope
{
    public static class PooledDisposeScopeExtensions
    {
        #region PooledList

        public static PooledList<T> ToPooledListScope<T>(this IEnumerable<T> items)
            => new PooledList<T>(items).RegisterDisposeScope();

        public static PooledList<T> ToPooledListScope<T>(this T[] array)
            => new PooledList<T>(array.AsSpan()).RegisterDisposeScope();

        public static PooledList<T> ToPooledListScope<T>(this ReadOnlySpan<T> span)
            => new PooledList<T>(span).RegisterDisposeScope();

        public static PooledList<T> ToPooledListScope<T>(this Span<T> span)
            => new PooledList<T>(span).RegisterDisposeScope();

        public static PooledList<T> ToPooledListScope<T>(this ReadOnlyMemory<T> memory)
            => new PooledList<T>(memory.Span).RegisterDisposeScope();

        public static PooledList<T> ToPooledListScope<T>(this Memory<T> memory)
            => new PooledList<T>(memory.Span).RegisterDisposeScope();

        #endregion

        #region PooledDictionary

        /// <summary>
        /// Creates a <see cref="PooledDictionary{TKey,TValue}"/> from an <see cref="IEnumerable{TSource}"/> according to specified 
        /// key selector and element selector functions, as well as a comparer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledDictionary<TKey, TValue> ToPooledDictionaryScope<TSource, TKey, TValue>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector,
            IEqualityComparer<TKey> comparer = null)
        {
            return source.ToPooledDictionary(keySelector, valueSelector, comparer).RegisterDisposeScope();
        }

        /// <summary>
        /// Creates a <see cref="PooledDictionary{TKey,TValue}"/> from a <see cref="ReadOnlySpan{TSource}"/> according to specified 
        /// key selector and element selector functions, as well as a comparer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledDictionary<TKey, TValue> ToPooledDictionaryScope<TSource, TKey, TValue>(
            this ReadOnlySpan<TSource> source,
            Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector,
            IEqualityComparer<TKey> comparer = null)
        {
            return source.ToPooledDictionary(keySelector, valueSelector, comparer).RegisterDisposeScope();
        }

        /// <summary>
        /// Creates a <see cref="PooledDictionary{TKey,TValue}"/> from a <see cref="Span{TSource}"/> according to specified 
        /// key selector and element selector functions, as well as a comparer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledDictionary<TKey, TValue> ToPooledDictionaryScope<TSource, TKey, TValue>(
            this Span<TSource> source,
            Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector, IEqualityComparer<TKey> comparer)
        {
            return source.ToPooledDictionary(keySelector, valueSelector, comparer).RegisterDisposeScope();
        }

        /// <summary>
        /// Creates a <see cref="PooledDictionary{TKey,TValue}"/> from a <see cref="ReadOnlyMemory{TSource}"/> according to specified 
        /// key selector and element selector functions, as well as a comparer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledDictionary<TKey, TValue> ToPooledDictionaryScope<TSource, TKey, TValue>(
            this ReadOnlyMemory<TSource> source,
            Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector, IEqualityComparer<TKey> comparer)
        {
            return source.ToPooledDictionary(keySelector, valueSelector, comparer).RegisterDisposeScope();
        }

        /// <summary>
        /// Creates a <see cref="PooledDictionary{TKey,TValue}"/> from a <see cref="Memory{TSource}"/> according to specified 
        /// key selector and element selector functions, as well as a comparer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledDictionary<TKey, TValue> ToPooledDictionaryScope<TSource, TKey, TValue>(
            this Memory<TSource> source,
            Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector, IEqualityComparer<TKey> comparer)
        {
            return source.ToPooledDictionary(keySelector, valueSelector, comparer).RegisterDisposeScope();
        }

        /// <summary>
        /// Creates a <see cref="PooledDictionary{TKey,TValue}"/> from an <see cref="IEnumerable{TSource}"/> according to specified 
        /// key selector and comparer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledDictionary<TKey, TSource> ToPooledDictionaryScope<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
        {
            return source.ToPooledDictionary(keySelector, comparer).RegisterDisposeScope();
        }

        /// <summary>
        /// Creates a <see cref="PooledDictionary{TKey,TValue}"/> from an <see cref="ReadOnlySpan{TSource}"/> according to specified 
        /// key selector and comparer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledDictionary<TKey, TSource> ToPooledDictionaryScope<TSource, TKey>(
            this ReadOnlySpan<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
        {
            return source.ToPooledDictionary(keySelector, comparer).RegisterDisposeScope();
        }

        /// <summary>
        /// Creates a <see cref="PooledDictionary{TKey,TValue}"/> from an <see cref="Span{TSource}"/> according to specified 
        /// key selector and comparer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledDictionary<TKey, TSource> ToPooledDictionaryScope<TSource, TKey>(this Span<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
        {
            return source.ToPooledDictionary(keySelector, comparer).RegisterDisposeScope();
        }

        /// <summary>
        /// Creates a <see cref="PooledDictionary{TKey,TValue}"/> from an <see cref="ReadOnlyMemory{TSource}"/> according to specified 
        /// key selector and comparer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledDictionary<TKey, TSource> ToPooledDictionaryScope<TSource, TKey>(
            this ReadOnlyMemory<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
        {
            return source.ToPooledDictionary(keySelector, comparer).RegisterDisposeScope();
        }

        /// <summary>
        /// Creates a <see cref="PooledDictionary{TKey,TValue}"/> from an <see cref="Memory{TSource}"/> according to specified 
        /// key selector and comparer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledDictionary<TKey, TSource> ToPooledDictionaryScope<TSource, TKey>(
            this Memory<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
        {
            return source.ToPooledDictionary(keySelector, comparer).RegisterDisposeScope();
        }

        /// <summary>
        /// Creates a <see cref="PooledDictionary{TKey,TValue}"/> from a sequence of key/value tuples.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledDictionary<TKey, TValue> ToPooledDictionaryScope<TKey, TValue>(
            this IEnumerable<(TKey, TValue)> source,
            IEqualityComparer<TKey> comparer = null)
        {
            return new PooledDictionary<TKey, TValue>(source, comparer).RegisterDisposeScope();
        }

        /// <summary>
        /// Creates a <see cref="PooledDictionary{TKey,TValue}"/> from a sequence of KeyValuePair values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledDictionary<TKey, TValue> ToPooledDictionaryScope<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> source,
            IEqualityComparer<TKey> comparer = null)
        {
            return new PooledDictionary<TKey, TValue>(source, comparer).RegisterDisposeScope();
        }

        /// <summary>
        /// Creates a <see cref="PooledDictionary{TKey,TValue}"/> from a sequence of key/value tuples.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledDictionary<TKey, TValue> ToPooledDictionaryScope<TKey, TValue>(
            this IEnumerable<Tuple<TKey, TValue>> source,
            IEqualityComparer<TKey> comparer = null)
        {
            return source.ToPooledDictionary(comparer);
        }

        /// <summary>
        /// Creates a <see cref="PooledDictionary{TKey,TValue}"/> from a span of key/value tuples.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledDictionary<TKey, TValue> ToPooledDictionaryScope<TKey, TValue>(
            this ReadOnlySpan<(TKey, TValue)> source,
            IEqualityComparer<TKey> comparer = null)
        {
            return new PooledDictionary<TKey, TValue>(source, comparer).RegisterDisposeScope();
        }

        /// <summary>
        /// Creates a <see cref="PooledDictionary{TKey,TValue}"/> from a span of key/value tuples.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledDictionary<TKey, TValue> ToPooledDictionaryScope<TKey, TValue>(
            this Span<(TKey, TValue)> source,
            IEqualityComparer<TKey> comparer = null)
        {
            return new PooledDictionary<TKey, TValue>(source, comparer).RegisterDisposeScope();
        }

        #endregion

        #region PooledSet

        public static PooledSet<T> ToPooledSetScope<T>(this IEnumerable<T> source,
            IEqualityComparer<T> comparer = null)
            => new PooledSet<T>(source, comparer).RegisterDisposeScope();

        public static PooledSet<T> ToPooledSetScope<T>(this Span<T> source, IEqualityComparer<T> comparer = null)
            => new PooledSet<T>(source, comparer).RegisterDisposeScope();

        public static PooledSet<T> ToPooledSetScope<T>(this ReadOnlySpan<T> source,
            IEqualityComparer<T> comparer = null)
            => new PooledSet<T>(source, comparer).RegisterDisposeScope();

        public static PooledSet<T> ToPooledSetScope<T>(this Memory<T> source, IEqualityComparer<T> comparer = null)
            => new PooledSet<T>(source.Span, comparer).RegisterDisposeScope();

        public static PooledSet<T> ToPooledSetScope<T>(this ReadOnlyMemory<T> source,
            IEqualityComparer<T> comparer = null)
            => new PooledSet<T>(source.Span, comparer).RegisterDisposeScope();

        #endregion

        #region PooledStack

        /// <summary>
        /// Creates an instance of PooledStack from the given items.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledStack<T> ToPooledStackScope<T>(this IEnumerable<T> items)
            => new PooledStack<T>(items).RegisterDisposeScope();

        /// <summary>
        /// Creates an instance of PooledStack from the given items.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledStack<T> ToPooledStackScope<T>(this T[] array)
            => new PooledStack<T>(array.AsSpan()).RegisterDisposeScope();

        /// <summary>
        /// Creates an instance of PooledStack from the given items.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledStack<T> ToPooledStackScope<T>(this ReadOnlySpan<T> span)
            => new PooledStack<T>(span).RegisterDisposeScope();

        /// <summary>
        /// Creates an instance of PooledStack from the given items.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledStack<T> ToPooledStackScope<T>(this Span<T> span)
            => new PooledStack<T>(span).RegisterDisposeScope();

        /// <summary>
        /// Creates an instance of PooledStack from the given items.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledStack<T> ToPooledStackScope<T>(this ReadOnlyMemory<T> memory)
            => new PooledStack<T>(memory.Span).RegisterDisposeScope();

        /// <summary>
        /// Creates an instance of PooledStack from the given items.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledStack<T> ToPooledStackScope<T>(this Memory<T> memory)
            => new PooledStack<T>(memory.Span).RegisterDisposeScope();

        #endregion

        #region PooledQueue

        /// <summary>
        /// Creates an instance of PooledQueue from the given items.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledQueue<T> ToPooledQueueScope<T>(this IEnumerable<T> items)
            => new PooledQueue<T>(items).RegisterDisposeScope();

        /// <summary>
        /// Creates an instance of PooledQueue from the given items.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledQueue<T> ToPooledQueueScope<T>(this ReadOnlySpan<T> span)
            => new PooledQueue<T>(span).RegisterDisposeScope();

        /// <summary>
        /// Creates an instance of PooledQueue from the given items.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledQueue<T> ToPooledQueueScope<T>(this Span<T> span)
            => new PooledQueue<T>(span).RegisterDisposeScope();

        /// <summary>
        /// Creates an instance of PooledQueue from the given items.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledQueue<T> ToPooledQueueScope<T>(this ReadOnlyMemory<T> memory)
            => new PooledQueue<T>(memory.Span).RegisterDisposeScope();

        /// <summary>
        /// Creates an instance of PooledQueue from the given items.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledQueue<T> ToPooledQueueScope<T>(this Memory<T> memory)
            => new PooledQueue<T>(memory.Span).RegisterDisposeScope();

        /// <summary>
        /// Creates an instance of PooledQueue from the given items.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledQueue<T> ToPooledQueueScope<T>(this T[] array)
            => new PooledQueue<T>(array.AsSpan()).RegisterDisposeScope();

        #endregion
    }
}
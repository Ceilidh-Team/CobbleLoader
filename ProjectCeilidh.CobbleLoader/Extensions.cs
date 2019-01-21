using System;
using System.Collections.Generic;

namespace ProjectCeilidh.CobbleLoader
{
    internal static class Extensions
    {
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
        {
            key = pair.Key;
            value = pair.Value;
        }
        
        public static IEnumerable<T> RecursiveUnroll<T>(this T start, Func<T, IEnumerable<T>> func)
        {
            var stack = new Stack<T>(new[]{ start });
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current;
                foreach (var next in func(current))
                    stack.Push(next);
            }
        }
    }
}
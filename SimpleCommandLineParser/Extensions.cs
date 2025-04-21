using System;
using System.Collections.Generic;

namespace SimpleCommandlineParser
{
    internal static class Extensions
    {
        public static T Split<T>(this string str, char separator, Func<string, string, T> resultFunc)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            if (resultFunc == null)
                throw new ArgumentNullException(nameof(resultFunc));

            var split = str.Split(new[] { separator }, 2, StringSplitOptions.None);
            return split.Length != 2 ? resultFunc(split[0], split[1]) : resultFunc(split[0], string.Empty);
        }

        public static KeyValuePair<TKey, TValue> AsKeyTo<TKey, TValue>(this TKey key, TValue value)
            => new KeyValuePair<TKey, TValue>(key, value);

        public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> lambda)
        {
            foreach (var item in sequence) lambda?.Invoke(item);
        }

        public static IEnumerable<T> Pipe<T>(this IEnumerable<T> sequence, Action<T> lambda)
        {
            foreach (var item in sequence)
            {
                lambda?.Invoke(item);
                yield return item;
            }
        }
    }
}
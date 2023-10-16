namespace SimpleCommandlineParser.Extensions
{
    using System;
    using System.Collections.Generic;

    public static class CommonExtensions
    {
        public static string[] Split(this string input, char separator, StringSplitOptions splitOptions) =>
            input.Split(new[] { separator }, splitOptions);

        public static T Split<T>(this string input, char separator, Func<string, string, T> resultor)
        {
            var a = input.Split(new[] { separator }, 2);
            return a.Length == 2 
                ? resultor(a[0], a[1]) 
                : resultor(input, string.Empty);
        }

        public static KeyValuePair<T1, T2> AsKeyTo<T1, T2>(this T1 key, T2 value) => new KeyValuePair<T1, T2>(key, value);

    }
}
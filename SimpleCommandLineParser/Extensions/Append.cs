using System.Collections.Generic;

namespace SimpleCommandlineParser.Extensions
{
    static partial class MoreEnumerable
    {
        public static IEnumerable<T> Append<T>(this IEnumerable<T> e, T extra)
        {
            foreach (var elm in e) yield return elm;
            yield return extra;
        }
    }
}
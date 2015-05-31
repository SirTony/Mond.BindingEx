using System;
using System.Collections.Generic;
using System.Linq;

namespace Mond.BindingEx
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<T> Reject<T>( this IEnumerable<T> enumerable, Func<T, bool> predicate )
        {
            return enumerable.Where( x => !predicate( x ) );
        }
    }
}

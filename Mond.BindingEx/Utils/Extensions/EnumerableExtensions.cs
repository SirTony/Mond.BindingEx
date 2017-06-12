using System;
using System.Collections.Generic;
using System.Linq;

namespace Mond.BindingEx.Utils.Extensions
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<T> Reject<T>(
            this IEnumerable<T> enumerable,
            Predicate<T> predicate ) => enumerable.Where( x => !predicate( x ) );

        public static IEnumerable<T> Concat<T>( this IEnumerable<T> enumerable, T another )
        {
            foreach( var item in enumerable ) yield return item;

            yield return another;
        }
    }
}

using System;
using System.Collections.Generic;

namespace Mond.BindingEx
{
    internal static class ArrayExtensions
    {
        public static IEnumerable<object> AsEnumerable( this Array array )
        {
            foreach( var item in array )
                yield return item;
        }
    }
}

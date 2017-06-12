using System;

namespace Mond.BindingEx
{
    internal static class StringExtensions
    {
        public static string With( this string format, params object[] args ) => String.Format( format, args );
    }
}

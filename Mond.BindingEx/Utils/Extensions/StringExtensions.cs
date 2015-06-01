using System;

namespace Mond.BindingEx
{
    internal static class StringExtensions
    {
        public static string With( this string format, params object[] args )
        {
            return String.Format( format, args );
        }
    }
}

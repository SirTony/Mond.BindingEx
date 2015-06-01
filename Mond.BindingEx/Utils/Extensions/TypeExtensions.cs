using System;

namespace Mond.BindingEx
{
    internal static class TypeExtensions
    {
        public static bool IsStruct( this Type type )
        {
            return type.IsValueType && !type.IsPrimitive && !type.IsEnum && type != typeof( decimal );
        }
    }
}

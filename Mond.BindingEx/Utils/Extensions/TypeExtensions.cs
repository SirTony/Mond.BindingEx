using System;
using System.Reflection;

namespace Mond.BindingEx.Utils.Extensions
{
    internal static class TypeExtensions
    {
        public static bool IsStruct( this Type type )
            => type.IsValueType &&
               !type.IsPrimitive &&
               !type.IsEnum &&
               ( type != typeof( decimal ) );

        public static string GetName( this Type type )
            => type.GetCustomAttribute<MondAliasAttribute>( true )?.Name ??
               ( type.IsGenericType ? type.Name.Substring( 0, type.Name.IndexOf( '`' ) ) : type.Name );
    }
}

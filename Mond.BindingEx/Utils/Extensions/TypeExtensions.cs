using System;
using System.Reflection;

namespace Mond.BindingEx.Utils.Extensions
{
    internal static class TypeExtensions
    {
        public static bool IsStruct( this Type type )
        {
            var info = type.GetTypeInfo();
            return info.IsValueType && !info.IsPrimitive && !info.IsEnum && ( type != typeof( decimal ) );
        }

        public static string GetName( this Type type )
        {
            var info = type.GetTypeInfo();
            return info.GetCustomAttribute<MondAliasAttribute>( true )?.Name ??
                   ( info.IsGenericType && type.Name.Contains( "`" )
                       ? type.Name.Substring( 0, type.Name.IndexOf( "`", StringComparison.Ordinal ) )
                       : type.Name );
        }
    }
}

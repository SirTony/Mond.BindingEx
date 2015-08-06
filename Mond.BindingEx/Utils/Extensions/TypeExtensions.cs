using System;
using System.Reflection;

namespace Mond.BindingEx
{
    internal static class TypeExtensions
    {
        public static bool IsStruct( this Type type )
        {
            return type.IsValueType && !type.IsPrimitive && !type.IsEnum && type != typeof( decimal );
        }

        public static string GetName( this Type type )
        {
            var alias = type.GetCustomAttribute<MondAliasAttribute>( true );

            if( alias != null )
                return alias.Name;

            return type.IsGenericType ? type.Name.Substring( 0, type.Name.IndexOf( '`' ) ) : type.Name;
        }
    }
}

using System;
using System.Linq;
using System.Reflection;

namespace Mond.BindingEx
{
    internal static class BindingUtils
    {
        private struct ReflectedMember<T> where T : MemberInfo
        {
            public bool Matched;
            public T Method;
            public Type[] Types;
        }

        public static MondFunction CreateConstructorShim( Type type, MondValue prototype )
        {
            return delegate( MondState state, MondValue[] args )
            {
                var target = GetTarget<ConstructorInfo>( type, null, args, BindingFlags.Public | BindingFlags.Instance );
                var values = TypeConverter.MarshalToClr( args, target.Types );
                var result = new MondValue( state );
                result.UserData = target.Method.Invoke( values );
                result.Prototype = prototype;

                return result;
            };
        }

        public static MondFunction CreateStaticMethodShim( Type type, string name )
        {
            return delegate( MondState state, MondValue[] args )
            {
                var target = GetTarget<MethodInfo>( type, name, args, BindingFlags.Public | BindingFlags.Static );
                var values = TypeConverter.MarshalToClr( args, target.Types );
                var result = target.Method.Invoke( null, values );

                if( target.Method.ReturnType == typeof( void ) )
                    return MondValue.Undefined;

                var mondType = TypeConverter.ToMondType( result.GetType() );
                return TypeConverter.MarshalToMond( result, mondType );
            };
        }

        public static MondInstanceFunction CreateInstanceMethodShim( Type type, string name )
        {
            return delegate( MondState state, MondValue instance, MondValue[] args )
            {
                var target = GetTarget<MethodInfo>( type, name, args, BindingFlags.Public | BindingFlags.Instance );
                var values = TypeConverter.MarshalToClr( args, target.Types );
                var result = target.Method.Invoke( instance.UserData, values );

                if( target.Method.ReflectedType == typeof( void ) )
                    return MondValue.Undefined;

                var mondType = TypeConverter.ToMondType( result.GetType() );
                return TypeConverter.MarshalToMond( result, mondType );
            };
        }

        private static Func<T, ReflectedMember<T>> CreateTargetMatcher<T>( MondValue[] args, string name ) where T : MemberInfo
        {
            if( typeof( T ) != typeof( ConstructorInfo ) && typeof( T ) != typeof( MethodInfo ) )
                throw new ArgumentException( "Generic argument must be either ConstructorInfo or MethodInfo", "T" );

            return delegate( T member )
            {
                if( name != null )
                {
                    if( member.Name != name )
                        return new ReflectedMember<T> { Matched = false };
                }

                var all = null as ParameterInfo[];

                if( member is ConstructorInfo )
                    all = ( member as ConstructorInfo ).GetParameters();

                if( member is MethodInfo )
                    all = ( member as MethodInfo ).GetParameters();

                var required = all.Reject( p => p.IsOptional ).ToArray();

                if( args.Length == 0 )
                    return new ReflectedMember<T> { Matched = required.Length == 0, Method = member, Types = Type.EmptyTypes };

                if( required.Length == args.Length && TypeConverter.MatchTypes( args, required.Select( p => p.ParameterType ).ToArray() ) )
                    return new ReflectedMember<T> { Matched = true, Method = member, Types = required.Select( p => p.ParameterType ).ToArray() };

                if( all.Length == args.Length && TypeConverter.MatchTypes( args, all.Select( p => p.ParameterType ).ToArray() ) )
                    return new ReflectedMember<T> { Matched = true, Method = member, Types = all.Select( p => p.ParameterType ).ToArray() };

                return new ReflectedMember<T> { Matched = false };
            };
        }

        private static ReflectedMember<T> GetTarget<T>( Type type, string name, MondValue[] args, BindingFlags flags ) where T : MemberInfo
        {
            if( typeof( T ) != typeof( ConstructorInfo ) && typeof( T ) != typeof( MethodInfo ) )
                throw new ArgumentException( "Generic argument must be either ConstructorInfo or MethodInfo", "T" );

            var matcher = CreateTargetMatcher<T>( args, name );
            var members = null as MemberInfo[];

            if( typeof( T ) == typeof( ConstructorInfo ) )
                members = type.GetConstructors( flags );

            if( typeof( T ) == typeof( MethodInfo ) )
                members = type.GetMethods( flags );

            var matched = members.Select( m => matcher( (T)m ) ).Where( m => m.Matched );

            if( matched.Count() > 1 )
                throw new AmbiguousMatchException( "More than one {0} in {1} matches the argument list".With( typeof( T ) == typeof( ConstructorInfo ) ? "constructor" : "method", type.Name ) );

            if( matched.Count() == 0 )
                throw new MissingMethodException( "No {0} in {1} matches the argument list".With( typeof( T ) == typeof( ConstructorInfo ) ? "constructor" : "method", type.Name ) );

            return matched.First();
        }
    }
}

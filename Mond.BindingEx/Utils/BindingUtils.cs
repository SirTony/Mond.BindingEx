using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Mond.BindingEx.Comparers;
using Mond.BindingEx.Utils.Extensions;

namespace Mond.BindingEx.Utils
{
    internal static class BindingUtils
    {
        public static MondFunction CreateConstructorShim( Type type, MondValue prototype, MondBindingOptions options )
            => delegate( MondState state, MondValue[] args )
               {
                   var target = BindingUtils.GetTarget<ConstructorInfo>(
                       type,
                       null,
                       args,
                       BindingFlags.Public | BindingFlags.Instance,
                       options );
                   var values = TypeConverter.MarshalToClr( args, target.Types, state );
                   return new MondValue( state )
                   {
                       UserData = target.Method.Invoke( values ),
                       Prototype = prototype
                   };
               };

        public static MondFunction CreateStaticMethodShim( Type type, string name, MondBindingOptions options )
            => delegate( MondState state, MondValue[] args )
               {
                   var target = BindingUtils.GetTarget<MethodInfo>(
                       type,
                       name,
                       args,
                       BindingFlags.Public | BindingFlags.Static,
                       options );
                   var values = TypeConverter.MarshalToClr( args, target.Types, state );
                   var result = target.Method.Invoke( null, values );

                   if( target.Method.ReturnType == typeof( void ) )
                       return MondValue.Undefined;

                   if( result == null ) return MondValue.Null;

                   var mondType = TypeConverter.ToMondType( result.GetType() );
                   return TypeConverter.MarshalToMond( result, mondType );
               };

        public static MondFunction CreateStaticMethodShim( Delegate function, MondBindingOptions options )
            => delegate( MondState state, MondValue[] args )
               {
                   var matcher = BindingUtils.CreateTargetMatcher<Delegate>( args, null, options );
                   var matched = function.GetInvocationList().Select( matcher ).Where( m => m.Matched );

                   var reflectedMembers = matched as ReflectedMember<Delegate>[] ?? matched.ToArray();
                   if( reflectedMembers.Length > 1 )
                   {
                       throw new AmbiguousMatchException(
                           "More than one delegate in the invokation list matches the argument list" );
                   }

                   if( reflectedMembers.Length == 0 )
                   {
                       throw new MissingMethodException(
                           "No delegate in the invokation list matches the argument list" );
                   }

                   var target = reflectedMembers[0];
                   var values = TypeConverter.MarshalToClr( args, target.Types, state );
                   var result = target.Method.DynamicInvoke( values );

                   if( target.Method.Method.ReturnType == typeof( void ) )
                       return MondValue.Undefined;

                   if( result == null ) return MondValue.Null;

                   var mondType = TypeConverter.ToMondType( result.GetType() );
                   return TypeConverter.MarshalToMond( result, mondType );
               };

        public static MondFunction CreateStaticOverloadGroupShim(
            IEnumerable<MethodInfo> methods,
            MondBindingOptions options )
            => delegate( MondState state, MondValue[] args )
               {
                   var matcher = BindingUtils.CreateTargetMatcher<MethodInfo>( args, null, options );
                   var matched = methods.Select( matcher ).Where( m => m.Matched );

                   var reflectedMembers = matched as ReflectedMember<MethodInfo>[] ?? matched.ToArray();
                   if( reflectedMembers.Length > 1 )
                   {
                       throw new AmbiguousMatchException(
                           "More than one delegate in the invokation list matches the argument list" );
                   }

                   if( reflectedMembers.Length == 0 )
                   {
                       throw new MissingMethodException(
                           "No delegate in the invokation list matches the argument list" );
                   }

                   var target = reflectedMembers[0];
                   var values = TypeConverter.MarshalToClr( args, target.Types, state );
                   var result = target.Method.Invoke( null, values );

                   if( target.Method.ReturnType == typeof( void ) )
                       return MondValue.Undefined;

                   if( result == null ) return MondValue.Null;

                   var mondType = TypeConverter.ToMondType( result.GetType() );
                   return TypeConverter.MarshalToMond( result, mondType );
               };

        public static MondInstanceFunction CreateInstanceMethodShim(
            Type type,
            string name,
            MondBindingOptions options )
            => delegate( MondState state, MondValue instance, MondValue[] args )
               {
                   // Remove the object instance from the argument list.
                   // This is primarily to prevent argument mismatch exceptions
                   // when the Mond runtime tries to dispatch a metamethod.
                   if( ( args != null ) && ( args.Length >= 1 ) && ( args[0] == instance ) )
                       args = args.Skip( 1 ).ToArray();

                   var target = BindingUtils.GetTarget<MethodInfo>(
                       type,
                       name,
                       args,
                       BindingFlags.Public | BindingFlags.Instance,
                       options );
                   var values = TypeConverter.MarshalToClr( args, target.Types, state );
                   var result = target.Method.Invoke( instance.UserData, values );

                   if( target.Method.ReturnType == typeof( void ) )
                       return MondValue.Undefined;

                   if( result == null ) return MondValue.Null;

                   var mondType = TypeConverter.ToMondType( result.GetType() );
                   return TypeConverter.MarshalToMond( result, mondType );
               };

        public static MondInstanceFunction CreateInstanceOverloadGroupShim(
            IEnumerable<MethodInfo> methods,
            MondBindingOptions options )
            => delegate( MondState state, MondValue instance, MondValue[] args )
               {
                   // Remove the object instance from the argument list.
                   // This is primarily to prevent argument mismatch exceptions
                   // when the Mond runtime tries to dispatch a metamethod.
                   if( ( args != null ) && ( args.Length >= 1 ) && ( args[0] == instance ) )
                       args = args.Skip( 1 ).ToArray();

                   var matcher = BindingUtils.CreateTargetMatcher<MethodInfo>( args, null, options );
                   var matched = methods.Select( matcher ).Where( m => m.Matched );

                   var reflectedMembers = matched as ReflectedMember<MethodInfo>[] ?? matched.ToArray();
                   if( reflectedMembers.Length > 1 )
                   {
                       throw new AmbiguousMatchException(
                           "More than one delegate in the invokation list matches the argument list" );
                   }

                   if( reflectedMembers.Length == 0 )
                   {
                       throw new MissingMethodException(
                           "No delegate in the invokation list matches the argument list" );
                   }

                   var target = reflectedMembers[0];
                   var values = TypeConverter.MarshalToClr( args, target.Types, state );
                   var result = target.Method.Invoke( instance.UserData, values );

                   if( target.Method.ReturnType == typeof( void ) )
                       return MondValue.Undefined;

                   if( result == null ) return MondValue.Null;

                   var mondType = TypeConverter.ToMondType( result.GetType() );
                   return TypeConverter.MarshalToMond( result, mondType );
               };

        private static Func<T, ReflectedMember<T>> CreateTargetMatcher<T>(
            MondValue[] args,
            string name,
            MondBindingOptions options )
        {
            if( ( typeof( T ) != typeof( ConstructorInfo ) ) &&
                ( typeof( T ) != typeof( MethodInfo ) ) &&
                ( typeof( T ) != typeof( Delegate ) ) )
            {
                throw new ArgumentException(
                    "Generic argument must be either ConstructorInfo, MethodInfo, or Delegate" );
            }

            return delegate( T member )
                   {
                       if( name != null )
                       {
                           var memberName = null as string;

                           if( member is ConstructorInfo )
                               memberName = ( member as ConstructorInfo ).GetName( options );
                           else if( member is MethodInfo )
                               memberName = ( member as MethodInfo ).GetName( options );
                           else if( member is Delegate )
                               memberName = ( member as Delegate ).Method.GetName( options );

                           if( memberName != name )
                               return new ReflectedMember<T> { Matched = false };
                       }

                       var all = null as ParameterInfo[];

                       if( member is ConstructorInfo )
                           all = ( member as ConstructorInfo ).GetParameters();
                       else if( member is MethodInfo )
                           all = ( member as MethodInfo ).GetParameters();
                       else if( member is Delegate )
                           all = ( member as Delegate ).Method.GetParameters();

                       var required = all.Reject( p => p.IsOptional ).ToArray();

                       if( args.Length == 0 )
                       {
                           return new ReflectedMember<T>
                           {
                               Matched = required.Length == 0,
                               Method = member,
                               Types = Type.EmptyTypes
                           };
                       }

                       if( ( required.Length == args.Length ) &&
                           TypeConverter.MatchTypes( args, required.Select( p => p.ParameterType ).ToArray() ) )
                       {
                           return new ReflectedMember<T>
                           {
                               Matched = true,
                               Method = member,
                               Types = required.Select( p => p.ParameterType ).ToArray()
                           };
                       }

                       // ReSharper disable once PossibleNullReferenceException
                       if( ( all.Length == args.Length ) &&
                           TypeConverter.MatchTypes( args, all.Select( p => p.ParameterType ).ToArray() ) )
                       {
                           return new ReflectedMember<T>
                           {
                               Matched = true,
                               Method = member,
                               Types = all.Select( p => p.ParameterType ).ToArray()
                           };
                       }

                       return new ReflectedMember<T> { Matched = false };
                   };
        }

        [SuppressMessage( "ReSharper", "CoVariantArrayConversion" )]
        private static ReflectedMember<T> GetTarget<T>(
            Type type,
            string name,
            MondValue[] args,
            BindingFlags flags,
            MondBindingOptions options )
            where T : MemberInfo
        {
            if( ( typeof( T ) != typeof( ConstructorInfo ) ) && ( typeof( T ) != typeof( MethodInfo ) ) )
                throw new ArgumentException( "Generic argument must be either ConstructorInfo or MethodInfo", "T" );

            var matcher = BindingUtils.CreateTargetMatcher<T>( args, name, options );
            var members = null as MemberInfo[];

            if( typeof( T ) == typeof( ConstructorInfo ) )
                members = type.GetConstructors( flags );
            else if( typeof( T ) == typeof( MethodInfo ) )
                members = type.GetMethods( flags );

            // ReSharper disable once AssignNullToNotNullAttribute
            var matched = members.Select( m => matcher( (T)m ) )
                                 .Where( m => m.Matched )
                                 .Distinct( new NumericTypeComparer<T>() );

            var reflectedMembers = matched as ReflectedMember<T>[] ?? matched.ToArray();
            if( reflectedMembers.Length > 1 )
            {
                throw new AmbiguousMatchException(
                    $"More than one {( typeof( T ) == typeof( ConstructorInfo ) ? "constructor" : "method" )}" +
                    $" in {type.GetName()} matches the argument list" );
            }

            if( reflectedMembers.Length == 0 )
            {
                throw new MissingMethodException(
                    $"No {( typeof( T ) == typeof( ConstructorInfo ) ? "constructor" : "method" )} in" +
                    $" {type.GetName()} matches the argument list" );
            }

            return reflectedMembers.First();
        }
    }
}
